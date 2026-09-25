namespace Application.Configurations;

public sealed class EnvSettings
{
    public JwtSettings Jwt { get; set; } = new();
}
