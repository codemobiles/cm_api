using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using cm_api.Models;
using CryptoHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace cm_api.Controllers
{

    [Route("api/[Controller]")]
    public class AuthController : Controller
    {

        ILogger<AuthController> _logger;
        private readonly DatabaseContext context;
        private readonly IConfiguration config;

        public AuthController(ILogger<AuthController> logger, DatabaseContext context, IConfiguration config)
        {
            this.config = config;
            this.context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Users model)
        {
            try
            {
                model.Password = Crypto.HashPassword(model.Password);

                context.Users.Add(model);
                context.SaveChanges();

                return Ok(new { result = "ok", message = "register successfully" });
            }
            catch (Exception error)
            {
                _logger.LogError($"Log Register: {error}");
                return StatusCode(500, new { result = "failure", message = error });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Users model)
        {
            try
            {
                var result = context.Users.SingleOrDefault(u => u.Username == model.Username);
                if (result == null)
                {
                    return Unauthorized(new { token = "", message = "username invalid" });
                }
                else if (Crypto.VerifyHashedPassword(result.Password, model.Password))
                {
                    var token = BuildToken(result);
                    return Ok(new { token = token, message = "login successfully" });
                }

                return Unauthorized(new { token = "", message = "password invalid" });
            }
            catch (Exception error)
            {
                _logger.LogError($"Log Register: {error}");
                return StatusCode(500, new { result = "failure", message = error });
            }
        }

        private string BuildToken(Users user)
        {
            // key is case-sensitive
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, config["Jwt:Subject"]),
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("position", user.Position),
                // for testing [Authorize(Roles = "admin")]
                // new Claim("role", "admin"),
                // new Claim(ClaimTypes.Role, user.Position)
            };

            var expires = DateTime.Now.AddDays(Convert.ToDouble(config["Jwt:ExpireDay"]));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}