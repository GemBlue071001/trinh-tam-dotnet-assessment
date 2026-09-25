using Application.Repositories;
using Application.Services.Auth;
using Infrastructure.Persistence.Repositories;

namespace ECommerceApi.Startup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
