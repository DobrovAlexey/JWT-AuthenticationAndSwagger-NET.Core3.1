using System.Security.Claims;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Interfaces
{
    public interface IJwtTokenValidator
    {
        ClaimsPrincipal GetPrincipalFromToken(string token, string signingKey);
    }
}