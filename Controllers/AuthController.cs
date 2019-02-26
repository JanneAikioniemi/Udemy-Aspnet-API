using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository m_authRepository;
        private readonly IConfiguration m_config;

        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            m_config = config;
            m_authRepository = authRepository;
        }

        // public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegisterDto) // if not using [ApiController]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto) // if using [ApiController]
        {
            // validate request if not using [ApiController]
            // if (!ModelState.IsValid)
            // {
            //     return BadRequest(ModelState);
            // }


            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await m_authRepository.UserExists(userForRegisterDto.Username))
            {
                return BadRequest("Username already taken");
            }

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = await m_authRepository.Register(userToCreate, userForRegisterDto.Password);

            // TODO: use CreatedAtRoute
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await m_authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            // start creating token
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username),
            };

            // Create security key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(m_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // create actual token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // send token back to client
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
    }
}