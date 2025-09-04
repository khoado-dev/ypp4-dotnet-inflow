using Inflow.Dtos;
using Inflow.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

namespace InFlow.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _service.RegisterAsync(dto);

            return result.Success? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _service.LoginAsync(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var result = await _service.ForgotPasswordAsync(dto);

            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("verify-resetcode")]
        public async Task<IActionResult> VerifyResetCode(VerifyResetCodeDto dto)
        {
            var result = await _service.VerifyResetCodeAsync(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await _service.ResetPasswordAsync(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
