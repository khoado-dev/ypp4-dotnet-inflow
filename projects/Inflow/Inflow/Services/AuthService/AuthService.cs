using Inflow.Repositories.AccountRepo;
using Inflow.Services.EmailService;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Inflow.Dtos;
using Inflow.Models;

namespace Inflow.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _repo;
        private readonly IEmailService _emailService;

        public AuthService(IAccountRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            return !IsValidEmail(dto.Email)
                ? new AuthResponseDto { Success = false, Message = "Invalid email format" }
                : (await _repo.GetByEmailAsync(dto.Email)) != null
                    ? new AuthResponseDto { Success = false, Message = "Email already exists" }
                    : !IsValidPassword(dto.Password)
                        ? new AuthResponseDto { Success = false, Message = "Password must have 8 characters, uppercase, lowercase and numbers" }
                        : await CreateAccountAsync(dto);
        }

        private async Task<AuthResponseDto> CreateAccountAsync(RegisterDto dto)
        {
            var account = new Account
            {
                FirstName = dto.FirstName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = HashPassword(dto.Password),
            };

            await _repo.CreateAsync(account);
            return new AuthResponseDto { Success = true, Message = "Register success" };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var account = await _repo.GetByEmailAsync(dto.Email);
            return (account == null || account.PasswordHash != HashPassword(dto.Password))
                ? new AuthResponseDto { Success = false, Message = "Invalid credentials" }
                : new AuthResponseDto { Success = true, Message = "Login success" };
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var account = await _repo.GetByEmailAsync(dto.Email);
            if (account == null)
                return new AuthResponseDto { Success = false, Message = "Email not found" };

            var resetCode = new Random().Next(100000, 999999).ToString();
            account.ResetCode = resetCode;
            await _repo.UpdateAsync(account);

            await _emailService.SendEmailAsync(dto.Email,
                "Password Reset Code",
                $"Your reset code is: <b>{resetCode}</b>");

            return new AuthResponseDto { Success = true, Message = $"Reset code sent to {dto.Email}" };
        }

        public async Task<AuthResponseDto> VerifyResetCodeAsync(VerifyResetCodeDto dto)
        {
            var account = await _repo.GetByResetCodeAsync(dto.Email, dto.ResetCode);
            return account == null
                ? new AuthResponseDto { Success = false, Message = "Invalid reset code" }
                : new AuthResponseDto { Success = true, Message = "Validation successfully" };
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var account = await _repo.GetByResetCodeAsync(dto.Email, dto.ResetCode);
            return account == null
                ? new AuthResponseDto { Success = false, Message = "Invalid reset code" }
                : await ResetPasswordInternalAsync(account, dto.NewPassword);
        }

        private async Task<AuthResponseDto> ResetPasswordInternalAsync(Account account, string newPassword)
        {
            account.PasswordHash = HashPassword(newPassword);
            account.ResetCode = null;
            await _repo.UpdateAsync(account);

            return new AuthResponseDto { Success = true, Message = "Password reset successfully" };
        }

        // Utils
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit);
        }

        private bool IsValidEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

    }
}
