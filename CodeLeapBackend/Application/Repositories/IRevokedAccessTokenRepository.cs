using Domain.Entities;

namespace Application.Repositories;

public interface IRevokedAccessTokenRepository : IGenericRepository<RevokedAccessToken>
{
    Task<bool> IsRevoked(string jwtId);
}
