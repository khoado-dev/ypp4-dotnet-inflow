namespace Inflow.Services.AuthService
{
    public enum AuthMessageKey
    {
        InvalidEmail,
        EmailExists,
        WeakPassword,
        RegisterSuccess,
        InvalidCredentials,
        EmailNotFound,
        ResetCodeSent,
        InvalidResetCode,
        VerifySuccess,
        ResetPasswordSuccess,
        LoginSuccess
    }

    public static class AuthMessageExtensions
    {
        private static readonly Dictionary<AuthMessageKey, string> Messages = new()
        {
            [AuthMessageKey.InvalidEmail] = "Invalid email format",
            [AuthMessageKey.EmailExists] = "Email already exists",
            [AuthMessageKey.WeakPassword] = "Password must have 8 characters, uppercase, lowercase and numbers",
            [AuthMessageKey.RegisterSuccess] = "Register success",
            [AuthMessageKey.InvalidCredentials] = "Invalid credentials",
            [AuthMessageKey.EmailNotFound] = "Email not found",
            [AuthMessageKey.ResetCodeSent] = "Reset code sent to {0}",
            [AuthMessageKey.InvalidResetCode] = "Invalid reset code",
            [AuthMessageKey.VerifySuccess] = "Validation successfully",
            [AuthMessageKey.ResetPasswordSuccess] = "Password reset successfully",
            [AuthMessageKey.LoginSuccess] = "Login success"
        };

        public static string GetMessage(this AuthMessageKey key) =>
            Messages[key];
    }
}
