namespace ForgotPasswordProvider.Models;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = null!;
    public string ForgotPasswordVerificationCode { get; set; } = null!;
} 
