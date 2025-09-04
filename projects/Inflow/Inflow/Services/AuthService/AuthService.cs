using Inflow.Dtos;
using Inflow.Models;
using Inflow.Repositories.AccountRepo;
using Inflow.Services.EmailService;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

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
                ? new AuthResponseDto { Success = false, Message = AuthMessageKey.InvalidEmail.GetMessage() }
                : (await _repo.GetByEmailAsync(dto.Email)) != null
                    ? new AuthResponseDto { Success = false, Message = AuthMessageKey.EmailExists.GetMessage() }
                    : !IsValidPassword(dto.Password)
                        ? new AuthResponseDto { Success = false, Message = AuthMessageKey.WeakPassword.GetMessage() }
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
            return new AuthResponseDto { Success = true, Message = AuthMessageKey.RegisterSuccess.GetMessage() };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var account = await _repo.GetByEmailAsync(dto.Email);

            return (account == null || account.PasswordHash != HashPassword(dto.Password))
                ? new AuthResponseDto { Success = false, Message = AuthMessageKey.InvalidCredentials.GetMessage() }
                : new AuthResponseDto { Success = true, Message = AuthMessageKey.LoginSuccess.GetMessage() };
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var account = await _repo.GetByEmailAsync(dto.Email);
            if (account == null)
                return new AuthResponseDto { Success = false, Message = AuthMessageKey.EmailNotFound.GetMessage() };

            var resetCode = new Random().Next(100000, 999999).ToString();
            account.ResetCode = resetCode;
            await _repo.UpdateAsync(account);

            var emailSubject = "Password Reset Code";
            var emailBody = $"Your reset code is: <b>{resetCode}</b>";

            await _emailService.SendEmailAsync(dto.Email,
                emailSubject,
                emailBody);

            return new AuthResponseDto
            {
                Success = true,
                Message = string.Format(AuthMessageKey.ResetCodeSent.GetMessage(), dto.Email)
            };
        }

        public async Task<AuthResponseDto> VerifyResetCodeAsync(VerifyResetCodeDto dto)
        {
            var account = await _repo.GetByResetCodeAsync(dto.Email, dto.ResetCode);

            return account == null
                ? new AuthResponseDto { Success = false, Message = AuthMessageKey.InvalidResetCode.GetMessage() }
                : new AuthResponseDto { Success = true, Message = AuthMessageKey.VerifySuccess.GetMessage() };
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var account = await _repo.GetByResetCodeAsync(dto.Email, dto.ResetCode);

            return account == null
                ? new AuthResponseDto { Success = false, Message = AuthMessageKey.InvalidResetCode.GetMessage() }
                : await ResetPasswordInternalAsync(account, dto.NewPassword);
        }

        private async Task<AuthResponseDto> ResetPasswordInternalAsync(Account account, string newPassword)
        {
            account.PasswordHash = HashPassword(newPassword);
            account.ResetCode = null;
            await _repo.UpdateAsync(account);

            return new AuthResponseDto { Success = true, Message = AuthMessageKey.ResetPasswordSuccess.GetMessage() };
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
