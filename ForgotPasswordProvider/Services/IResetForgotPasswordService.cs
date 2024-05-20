using ForgotPasswordProvider.Models;
using Microsoft.AspNetCore.Http;

namespace ForgotPasswordProvider.Services
{
    public interface IResetForgotPasswordService
    {
        Task<bool> ResetPassword(ResetForgotPasswordRequest request);
        Task<ResetForgotPasswordRequest> UnpackResetForgotPasswordRequest(HttpRequest req);
    }
}