using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Models.Auth;
using Application.Models.Cart;
using Application.Models.Checkout;
using Application.Models.Product;
using Xunit;

namespace ECommerceApi.IntegrationTests;

public sealed class ECommerceApiTests(ECommerceApiFactory factory) : IClassFixture<ECommerceApiFactory>
{
    [Fact]
    public async Task RegisterLoginAndGetCurrentUser_ReturnsAuthenticatedProfile()
    {
        using var client = factory.CreateClient();
        var email = $"auth-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "Password@123",
            FullName = "Integration Customer"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "Password@123"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(login?.JwtToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.JwtToken);
        var currentUserResponse = await client.GetAsync("/api/auth/me");
        var currentUser = await currentUserResponse.Content.ReadFromJsonAsync<CurrentUserResponse>();

        Assert.Equal(HttpStatusCode.OK, currentUserResponse.StatusCode);
        Assert.NotNull(currentUser);
        Assert.Equal(email, currentUser!.Email);
        Assert.Equal("Customer", currentUser.Role);
        Assert.Equal("Integration Customer", currentUser.FullName);
    }

    [Fact]
    public async Task CustomerCanAddSeededProductToCartAndCheckout()
    {
        using var client = factory.CreateClient();
        var token = await RegisterAndLoginCustomer(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var products = await client.GetFromJsonAsync<PagedProductResponse>("/api/products?pageNumber=1&pageSize=10");
        var product = Assert.Single(products!.Items, item => item.Name == "MacBook Air M3");

        var addItemResponse = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = 1
        });
        Assert.Equal(HttpStatusCode.OK, addItemResponse.StatusCode);
        var cart = await addItemResponse.Content.ReadFromJsonAsync<CartResponse>();
        Assert.NotNull(cart);
        Assert.Single(cart!.Items);

        var checkoutResponse = await client.PostAsync("/api/cart/checkout", null);
        Assert.Equal(HttpStatusCode.Created, checkoutResponse.StatusCode);
        var order = await checkoutResponse.Content.ReadFromJsonAsync<CheckoutResponse>();

        Assert.NotNull(order);
        Assert.Equal(cart.CartId, order!.CartId);
        Assert.Equal(product.Price, order.TotalAmount);
        Assert.Single(order.Items);
    }

    private static async Task<string> RegisterAndLoginCustomer(HttpClient client)
    {
        var email = $"cart-{Guid.NewGuid():N}@example.com";
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "Password@123"
        });
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "Password@123"
        });
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return login!.JwtToken;
    }
}
