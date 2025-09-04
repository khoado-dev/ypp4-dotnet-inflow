using Inflow.Dtos;

namespace Inflow.Services.AuthService
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
        Task<AuthResponseDto> VerifyResetCodeAsync(VerifyResetCodeDto dto);
    }
}
