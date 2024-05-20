using ForgotPasswordProvider.Data.Entities;
using ForgotPasswordProvider.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForgotPasswordProvider.Services;

public class ResetForgotPasswordService(ILogger<ResetForgotPasswordService> logger, UserManager<UserEntitiy> userManager) : IResetForgotPasswordService
{
    private readonly ILogger<ResetForgotPasswordService> _logger = logger;
    private readonly UserManager<UserEntitiy> _userManager = userManager;

    public async Task<ResetForgotPasswordRequest> UnpackResetForgotPasswordRequest(HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var resetForgotPasswordRequest = JsonConvert.DeserializeObject<ResetForgotPasswordRequest>(body);

                if (resetForgotPasswordRequest != null)
                {
                    return resetForgotPasswordRequest;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ResetForgotPasswordService.UnpackResetForgotPasswordRequest() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> ResetPassword(ResetForgotPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                if (!string.IsNullOrEmpty(code))
                {
                    var result = await _userManager.ResetPasswordAsync(user, code, request.Password);

                    if (result.Succeeded)
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ResetForgotPasswordService.ResetPassword() :: {ex.Message}");
        }
        return false;
    }
}
