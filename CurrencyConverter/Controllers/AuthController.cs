using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Application.Models;
using CurrencyConverter.Infrastructure.JWT;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// User Login
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginReq loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var users = new Users().GetMockUsers();
            var user = users.FirstOrDefault(u => u.Username.Equals(loginRequest.UserName, StringComparison.OrdinalIgnoreCase) && u.Password == loginRequest.Password);

            if (user != null)
            {
                var clientId = Request.Headers["clientid"].FirstOrDefault();
                var token = new JWTTokenGenerator(_config).GenerateJwtToken(user.Username, clientId, user.Role);
                return Ok(new { Token = token });
            }
            return Unauthorized();
        }
    }
}
