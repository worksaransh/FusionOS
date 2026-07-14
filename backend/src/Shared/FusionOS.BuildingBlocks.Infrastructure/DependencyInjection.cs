using FusionOS.SharedKernel.Context;
using FusionOS.BuildingBlocks.Infrastructure.CurrentUser;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureBuildingBlocks(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        return services;
    }
}
