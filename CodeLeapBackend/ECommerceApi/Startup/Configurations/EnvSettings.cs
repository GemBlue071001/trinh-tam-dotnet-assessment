namespace ECommerceApi.Startup.Configurations;

public sealed class EnvSettings
{
    public JwtSettings Jwt { get; set; } = new();
}
