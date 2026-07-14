# Multi-stage build for FusionOS.Api.Host — 02_TECH_STACK.md §9.
# One image serves every module in the monolith today; per 03_SYSTEM_ARCHITECTURE.md
# each module is still internally partitioned so a future extraction into its own
# image/service is a deployment change, not a rewrite.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore FusionOS.sln
RUN dotnet publish src/Host/FusionOS.Api.Host/FusionOS.Api.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FusionOS.Api.Host.dll"]
