using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using GeoStream.Api.Infrastructure.Interfaces.Providers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GeoStream.Api.Infrastructure.Settings;

namespace GeoStream.Api.Infrastructure.Providers
{
    public class JwtTokenProvider : IJwtTokenProvider
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenProvider(IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor)
        {
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateAuthenticationToken(TimeSpan? expirationTime = null, params Claim[] claims)
        {
            var systemClaims = new List<Claim>();

            if (!claims.Any(c => c.Type == JwtRegisteredClaimNames.Sub))
            {
                systemClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, "System"));
            }

            var allClaims = systemClaims.Concat(claims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: allClaims,
                expires: expirationTime.HasValue ? DateTime.UtcNow.Add(expirationTime.Value) : null,
                signingCredentials: signingCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return token;
        }

        public string? GetUserAuthenticationToken()
        {
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }

        public (string userId, string userEmail, string userName) GetUserDataFromFromAuthenticationToken()
        {
            var accessToken = GetUserAuthenticationToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("User authentication token is missing or invalid.");
            }

            ClaimsPrincipal claimsPrincipal = ExtractClaimsFromAuthenticationToken(accessToken);
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userEmail = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var userName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            return (userId, userEmail, userName);

        }

        private ClaimsPrincipal ExtractClaimsFromAuthenticationToken(string token, bool validateLifetime = true)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateLifetime = validateLifetime,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidAudience = _jwtSettings.Audience,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key))
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token.");
                }

                return principal;
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid or expired token.", ex);
            }
        }
    }
}
