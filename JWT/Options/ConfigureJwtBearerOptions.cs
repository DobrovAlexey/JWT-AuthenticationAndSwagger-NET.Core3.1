using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Options
{
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly JwtIssuerOptions _jwtIssuerOptions;
        private readonly JwtOption _jwtOption;

        public ConfigureJwtBearerOptions(IOptions<JwtIssuerOptions> jwtIssuerOptions, IOptions<JwtOption> jwtOptions)
        {
            _jwtIssuerOptions = jwtIssuerOptions.Value;
            _jwtOption = jwtOptions.Value;
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            if (name == JwtBearerDefaults.AuthenticationScheme)
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuerOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtIssuerOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOption.SecretKey)),
                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };

                options.RequireHttpsMetadata = false;
                options.ClaimsIssuer = _jwtIssuerOptions.Issuer;
                options.TokenValidationParameters = tokenValidationParameters;
                options.SaveToken = true;
                options.IncludeErrorDetails = true;

                options.Events = new JwtBearerEvents
                {
                    // Invoked if exceptions are thrown during request processing.
                    // The exceptions will be re - thrown after this event unless suppressed.
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException)) context.Response.Headers.Add("Token-Expired", "true");

                        return Task.CompletedTask;
                    },

                    // Invoked before a challenge is sent back to the caller.
                    OnChallenge = async context =>
                    {
                        // Override the response status code.
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        // Emit the WWW-Authenticate header.
                        context.Response.Headers.Append(HeaderNames.WWWAuthenticate, context.Options.Challenge);

                        string message;

                        if (context.Response.Headers.ContainsKey("Token-Expired"))
                        {
                            message = "Срок действия токена истёк.";
                        }
                        else if (context.Error == "invalid_token")
                        {
                            message = "Неверный токен.";
                        }
                        else if (!string.IsNullOrEmpty(context.Error) && !string.IsNullOrEmpty(context.ErrorDescription))
                        {
                            message = $"{context.Error} {context.ErrorDescription}";
                        }
                        else if (!string.IsNullOrEmpty(context.Error))
                        {
                            message = context.Error;
                        }
                        else
                        {
                            message = "Нет доступа. Необходимо авторизоваться.";
                        }

                        var result = new ErrorMessage(message, StatusCodes.Status401Unauthorized);
                        await context.Response.WriteAsync(result.ToJson());

                        context.HandleResponse();
                    },

                    // Invoked if Authorization fails and results in a Forbidden response
                    OnForbidden = context =>
                    {
                        // Override the response status code.
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        // Emit the WWW-Authenticate header.
                        context.Response.Headers.Append(HeaderNames.WWWAuthenticate, context.Options.Challenge);

                        var result = new ErrorMessage("Нет доступа.", StatusCodes.Status403Forbidden);

                        context.Response.WriteAsync(result.ToJson());

                        return Task.CompletedTask;
                    },

                    // Invoked when a protocol message is first received.
                    OnMessageReceived = context =>
                    {
                        return Task.CompletedTask;
                    },

                    // Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            Configure(JwtBearerDefaults.AuthenticationScheme, options);
        }
    }
}
