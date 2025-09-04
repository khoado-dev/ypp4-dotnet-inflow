namespace Inflow.Dtos
{
    public class VerifyResetCodeDto
    {
        public string Email { get; set; } = string.Empty;
        public string ResetCode { get; set; } = string.Empty;
    }
}