using System;
using System.Security.Cryptography;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Interfaces;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Options;
using Microsoft.Extensions.Options;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Services
{
    public class RefreshTokenFactory : IRefreshTokenFactory
    {
        private readonly JwtOption _jwtOption;

        public RefreshTokenFactory(IOptions<JwtOption> jwtOption)
        {
            _jwtOption = jwtOption.Value;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[_jwtOption.SizeRefreshToken];
            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}