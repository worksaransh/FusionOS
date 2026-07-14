using AspNetCoreRateLimit;
using FusionOS.Api.Host.ErrorHandling;
using FusionOS.Api.Host.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.BuildingBlocks.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Fail fast on missing secrets (07_SECURITY.md — no secret ships a working
// default outside Development; appsettings.Development.json supplies dev-only
// placeholders, real environments must set ConnectionStrings__Postgres and
// Jwt__SigningKey via environment variables / a secret manager). -----------------
if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
{
    var missing = new List<string>();
    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Postgres")))
        missing.Add("ConnectionStrings:Postgres");
    if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"]))
        missing.Add("Jwt:SigningKey");

    if (missing.Count > 0)
    {
        throw new InvalidOperationException(
            $"Missing required configuration outside Development: {string.Join(", ", missing)}. " +
            "Set these via environment variables (e.g. Jwt__SigningKey) or a secret manager — " +
            "never commit real values to appsettings.json.");
    }
}

// --- Observability (09_CODING_STANDARDS.md §6) ------------------------------------
// Logs -> Grafana Loki (in addition to the console, so `docker compose logs` still
// works with nothing else running). Observability:LokiUrl is empty by default in
// appsettings.json, and Serilog's Loki sink degrades to a harmless self-log
// warning (not a crash) if the target is unreachable, so local dev without the
// observability stack up is unaffected either way.
var lokiUrl = builder.Configuration["Observability:LokiUrl"];
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("service_name", "fusionos-api")
        .WriteTo.Console();

    if (!string.IsNullOrWhiteSpace(lokiUrl))
    {
        configuration.WriteTo.GrafanaLoki(lokiUrl, labels: new[]
        {
            new LokiLabel { Key = "app", Value = "fusionos-api" },
        });
    }
});

// Traces + metrics both export via OTLP to Observability:OtlpEndpoint (the
// otel-collector service in docker-compose.yml) when it's configured; in
// Development they also fan out to the console exporter so `dotnet run`
// alone — no collector, no Grafana — still shows something. Previously this
// block registered AddAspNetCoreInstrumentation() with no exporter at all,
// so every trace was computed and then silently discarded (audit
// Observability finding). Metrics (ASP.NET Core request metrics + .NET
// runtime GC/thread-pool/exception counters) are new — there weren't any
// before this task — and are additionally exposed as a Prometheus scrape
// endpoint at /metrics, independent of whether OTLP metrics export is
// configured.
var otlpEndpoint = builder.Configuration["Observability:OtlpEndpoint"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceName: "fusionos-api",
        serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));

        if (builder.Environment.IsDevelopment())
            tracing.AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    });

// --- Cross-cutting building blocks -----------------------------------------------
builder.Services.AddInfrastructureBuildingBlocks();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.SectionName));
builder.Services.AddSingleton<FusionOS.SharedKernel.Events.IEventBus, KafkaEventBus>();
builder.Services.AddHostedService<KafkaConsumerHostedService>();

// --- AuthN/AuthZ (07_SECURITY.md) ------------------------------------------------
var jwtKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(jwtKey))
    jwtKey = "development-only-signing-key-do-not-use-in-any-real-environment";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "fusionos",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "fusionos-clients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

// Every endpoint requires an authenticated caller unless it opts out with
// [AllowAnonymous] (AuthController's login/refresh/register only) — closes the
// "wide-open by default" gap the audit flagged (no controller had [Authorize]).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// --- Rate limiting (AspNetCoreRateLimit was referenced but never configured —
// 07_SECURITY.md; brute-force login attempts had no throttling at all).
// GeneralRules in appsettings.json gives a global per-IP cap plus tighter
// per-minute limits on the three unauthenticated Auth endpoints. -----------------
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// --- CORS (only the origins operators explicitly configure; no wildcard) --------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// --- API platform (08_API_STANDARDS.md) ------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FusionOS API", Version = "v1" });
});
builder.Services.AddHealthChecks();

// RFC 7807 problem-details responses for every unhandled exception, per
// 08_API_STANDARDS.md §6 — previously documented but not actually wired up.
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddProblemDetails();

// --- Register every module (03_SYSTEM_ARCHITECTURE.md) ---------------------------
foreach (var module in ModuleRegistry.All)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

// Rate limiting runs before CORS/auth so even unauthenticated brute-force
// traffic against /core/auth/login gets throttled, not just already-signed-in
// callers (07_SECURITY.md).
app.UseIpRateLimiting();

app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Prometheus scrape target (docker-compose.yml's prometheus service polls
// this) — independent of the OTLP metrics export above, so metrics are
// visible even in a plain `dotnet run` with no collector configured.
app.MapPrometheusScrapingEndpoint();

foreach (var module in ModuleRegistry.All)
{
    module.MapEndpoints(app);
}

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
