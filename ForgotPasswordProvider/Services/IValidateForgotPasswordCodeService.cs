using ForgotPasswordProvider.Models;
using Microsoft.AspNetCore.Http;

namespace ForgotPasswordProvider.Services
{
    public interface IValidateForgotPasswordCodeService
    {
        Task<ValidateRequest> UnpackValidateRequestAsync(HttpRequest req);
        Task<bool> ValidateCodeAsync(ValidateRequest request);
    }
}