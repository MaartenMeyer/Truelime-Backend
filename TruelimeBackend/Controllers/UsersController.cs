using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TruelimeBackend.Helpers;
using TruelimeBackend.Models;
using TruelimeBackend.Services;

namespace TruelimeBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService userService;
        private IMapper mapper;
        private readonly Settings settings;

        public UsersController(IUserService userService, IMapper mapper, IOptions<Settings> settings)
        {
            this.userService = userService;
            this.mapper = mapper;
            this.settings = settings.Value;

        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserDto userIn)
        {
            var user = userService.Authenticate(userIn.Email, userIn.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Email or password is incorrect" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new {
                user.Id,
                user.Username,
                user.Email,
                Token = tokenString
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserDto userDto) {
            var user = mapper.Map<User>(userDto);

            if (userService.Get(user.Email) != null)
            {
                return Conflict(new {message = "Email is already taken"});
            }

            userService.Create(user, userDto.Password);

            return Ok(new { message = "User created" });
        }
    }
}