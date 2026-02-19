using System.Security.Claims;

namespace ServiceStarter.Api.Infrastructure.Auth;

public interface ITokenService
{
    string GenerateToken(string email, string role, IEnumerable<Claim>? additionalClaims = null);
}
