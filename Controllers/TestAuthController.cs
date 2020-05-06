using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestAuthController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        [Route("AllowAnonymous")]
        public bool AllowAnonymous()
        {
            return true;
        }

        [HttpGet]
        [Authorize]
        [Route("Authorize")]
        public bool Authorize()
        {
            return true;
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        [Route("RolesUser")]
        public bool RolesUser()
        {
            return true;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("RolesAdmin")]
        public bool RolesAdmin()
        {
            return true;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        [Route("MultipleAccess")]
        public bool MultipleAccess()
        {
            return true;
        }
    }
}