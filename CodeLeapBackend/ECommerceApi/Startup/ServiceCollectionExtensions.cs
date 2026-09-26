using Application.Repositories;
using Application.Services.Auth;
using Application.Services.Cart;
using Application.Services.Checkout;
using Application.Services.Product;
using Domain.Entities;
using Infrastructure.Persistence.Repositories;

namespace ECommerceApi.Startup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGenericRepository<Product>, GenericRepository<Product>>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
