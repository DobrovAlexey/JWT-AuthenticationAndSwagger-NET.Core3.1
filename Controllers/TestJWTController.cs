using System.Threading.Tasks;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Interfaces;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Models;
using JWT_AuthenticationAndSwagger_NET.Core3._1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class TestJwtController : ControllerBase
    {
        private readonly IJwtFactory _jwtFactory;
        private readonly IRefreshTokenFactory _refreshTokenFactory;

        public TestJwtController(IJwtFactory jwtFactory, IRefreshTokenFactory refreshTokenFactory)
        {
            _jwtFactory = jwtFactory;
            _refreshTokenFactory = refreshTokenFactory;
        }

        [HttpPost]
        [Route("AccessTokenGenerate")]
        public async Task<AccessToken> AccessToken(UserModel user)
        {
            var temp = await _jwtFactory.GenerateEncodedToken(user.Id, user.PhoneNumber, user.Email, user.LastName, user.FirstName, user.Region, user.City, user.Roles);
            return temp;
        }

        [HttpGet]
        [Route("AccessTokenValidate")]
        public long? AccessTokenValidate(string accessToken)
        {
            var id = _jwtFactory.ValidateToken(accessToken);
            return id;
        }

        [HttpGet]
        [Route("RefreshTokenGenerate")]
        public string RefreshToken()
        {
            var refreshToken = _refreshTokenFactory.GenerateRefreshToken();
            return refreshToken;
        }
    }
}