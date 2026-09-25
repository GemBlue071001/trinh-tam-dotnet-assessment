using Application.Services.Auth;

namespace ECommerceApi.Startup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
