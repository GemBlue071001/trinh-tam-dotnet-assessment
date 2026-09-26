using System.Text;
using Application.Configurations;
using Application.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceApi.Startup;

public static class JwtServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, EnvSettings envSettings)
    {
        var jwtSettings = envSettings.Jwt;

        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
            string.IsNullOrWhiteSpace(jwtSettings.Audience) ||
            string.IsNullOrWhiteSpace(jwtSettings.Key))
        {
            throw new InvalidOperationException("Jwt Issuer, Audience, and Key are required.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jwtId = context.Principal?.FindFirst("jti")?.Value;
                        if (string.IsNullOrWhiteSpace(jwtId))
                        {
                            context.Fail("JWT does not contain a token identifier.");
                            return;
                        }

                        var revokedTokenRepository = context.HttpContext.RequestServices
                            .GetRequiredService<IRevokedAccessTokenRepository>();

                        if (await revokedTokenRepository.IsRevoked(jwtId))
                        {
                            context.Fail("JWT has been logged out.");
                        }
                    }
                };
            });

        return services;
    }
}
