using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Services
{
    public class JwtTokenHandler : IJwtTokenHandler
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly ILogger<JwtTokenHandler> _logger;

        public JwtTokenHandler(ILogger<JwtTokenHandler> logger)
        {
            _logger = logger;
        }

        public string WriteToken(JwtSecurityToken jwt)
        {
            return _jwtSecurityTokenHandler.WriteToken(jwt);
        }


        public ClaimsPrincipal ValidateToken(string token, TokenValidationParameters tokenValidationParameters)
        {
            try
            {
                var principal =
                    _jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid access token.");
                }

                return principal;
            }
            catch (Exception e)
            {
                _logger.LogError($"Token validation failed: {e.Message}");
                throw new SecurityTokenValidationException("Token validation failed.");
            }
        }
    }
}