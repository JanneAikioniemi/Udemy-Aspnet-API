using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository m_authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            m_authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // TODO: validate request
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
    }
}