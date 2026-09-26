namespace Domain.Entities;

public sealed class RevokedAccessToken
{
    public Guid Id { get; set; }
    public string JwtId { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset RevokedAt { get; set; }
}
