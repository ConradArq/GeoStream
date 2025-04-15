using System.Security.Claims;

namespace GeoStream.Api.Infrastructure.Interfaces.Providers
{
    public interface IJwtTokenProvider
    {
        string GenerateAuthenticationToken(TimeSpan? expirationTime = null, params Claim[] claims);
        string? GetUserAuthenticationToken();
        (string userId, string userEmail, string userName) GetUserDataFromFromAuthenticationToken();
    }
}