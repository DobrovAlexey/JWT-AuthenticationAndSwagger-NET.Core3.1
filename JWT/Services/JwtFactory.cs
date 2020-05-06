using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Helpers;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Interfaces;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Models;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Options;
using Microsoft.Extensions.Options;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Services
{
    public sealed class JwtFactory : IJwtFactory
    {
        private readonly JwtOption _jwtOption;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IJwtTokenHandler _jwtTokenHandler;
        private readonly IJwtTokenValidator _jwtTokenValidator;

        public JwtFactory(IJwtTokenHandler jwtTokenHandler, IOptions<JwtIssuerOptions> jwtOptions,
            IJwtTokenValidator jwtTokenValidator, IOptions<JwtOption> jwtOption)
        {
            _jwtTokenHandler = jwtTokenHandler;
            _jwtOptions = jwtOptions.Value;
            _jwtTokenValidator = jwtTokenValidator;
            _jwtOption = jwtOption.Value;

            ThrowIfInvalidOptions(_jwtOptions);
        }

        public async Task<AccessToken> GenerateEncodedToken(long id, string phoneNumber, string email, string lastName,
            string firstName, string region, string city, string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iss, _jwtOptions.Issuer),
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, _jwtOptions.Audience),
                new Claim(JwtRegisteredClaimNames.Exp,
                    ToUnixExpDate(_jwtOptions.IssuedAt, _jwtOptions.ValidFor).ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(),
                    ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),

                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.UniqueName, phoneNumber),

                new Claim(Constants.Strings.JwtClaimIdentifiers.UserId, id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),

                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.MobilePhone, phoneNumber),

                new Claim(ClaimTypes.Surname, lastName),
                new Claim(ClaimTypes.Name, firstName),
                new Claim(ClaimTypes.GivenName, $"{firstName} {lastName}"),

                new Claim(ClaimTypes.StateOrProvince, region),
                new Claim(ClaimTypes.Locality, city),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim(Constants.Strings.JwtClaimIdentifiers.Roles, role));
            }

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                _jwtOptions.Issuer,
                _jwtOptions.Audience,
                claims,
                _jwtOptions.NotBefore,
                _jwtOptions.Expiration,
                _jwtOptions.SigningCredentials);

            return new AccessToken(_jwtTokenHandler.WriteToken(jwt), (int) _jwtOptions.ValidFor.TotalSeconds);
        }

        public long? ValidateToken(string token)
        {
            if (_jwtOption.SecretKey == null)
            {
                throw new ArgumentNullException(nameof(_jwtOption.SecretKey));
            }

            var claimsPrincipal = _jwtTokenValidator.GetPrincipalFromToken(token, _jwtOption.SecretKey);

            var userId = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId?.Value))
            {
                return null;
            }

            return long.Parse(userId.Value);
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime iatDate)
        {
            return (long) Math.Round(
                (iatDate.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }

        private static long ToUnixExpDate(DateTime iatDate, TimeSpan validForDate)
        {
            var date = iatDate.Add(validForDate);

            return (long) Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .TotalSeconds);
        }

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }
    }
}