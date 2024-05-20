using ForgotPasswordProvider.Models;
using Microsoft.AspNetCore.Http;

namespace ForgotPasswordProvider.Services
{
    public interface IForgotPasswordService
    {
        string GeneratedCode();
        EmailRequest GenerateEmailRequest(string email, string code);
        string GenerateServiceBusMessage(EmailRequest emailRequest);
        Task<bool> SaveForgotPasswordRequest(string email, string code);
        Task<ForgotPasswordRequest> UnpackForgotPasswordRequest(HttpRequest req);
    }
}