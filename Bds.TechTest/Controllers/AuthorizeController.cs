using Bds.TechTest.Dtos;
using Bds.TechTest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bds.TechTest.Controllers
{
    [Route("api/v1/[controller]")]
    public class AuthorizeController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IUserService userService;

        public AuthorizeController(IConfiguration configuration, IUserService userService)
        {
            this.configuration = configuration;
            this.userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Post([FromForm] SigninDto signinDto)
        {
            //This method returns user id from username and password.
            var user = await userService.GetUserByCredentials(signinDto.username, signinDto.password);
            if (user == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Id.ToString())
            };

            var expiresOn = DateTime.UtcNow.AddMinutes(30);
            var token = new JwtSecurityToken
            (
                issuer: configuration["TokenIssuer"],
                audience: configuration["TokenAudience"],
                claims: claims,
                expires: expiresOn,
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["SigningKey"])),
                        SecurityAlgorithms.HmacSha256)
            );


            return Ok(new {
                access_token = new JwtSecurityTokenHandler().WriteToken(token),
                expires_on = expiresOn
            });
        }
    }
}
