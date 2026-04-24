using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTOs;
using TiendaUCN.src.Application.DTOs.AuthDTO;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var message = await _userService.RegisterAsync(registerDTO);
            return Ok(new GenericResponse<string>("Usuario registrado exitosamente", message));
        }

        [HttpPost("email-verification")]
        public async Task<IActionResult> EmailVerification([FromBody] EmailVerificationDTO emailVerificationDTO)
        {
            await _userService.EmailVerificationAsync(emailVerificationDTO);
            return Ok(new GenericResponse<string>("Email verificado exitosamente", null));
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var token = await _userService.LoginAsync(loginDTO);
            return Ok(new GenericResponse<string>("Login exitoso", token));
        }

        [HttpPost("logout")]
        [Authorize(Roles ="Customer,Admin")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var message = await _userService.LogoutAsync(token);
            return Ok(new GenericResponse<string>(message, null));
        }
    }
}