using ForgotPasswordProvider.Data.Entities;
using ForgotPasswordProvider.Models;
using ForgotPasswordProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForgotPasswordProvider.Functions;

public class ResetForgotPassword(ILogger<ResetForgotPassword> logger, IResetForgotPasswordService resetForPaswordService)
{
    private readonly ILogger<ResetForgotPassword> _logger = logger;
    private readonly IResetForgotPasswordService _resetForPaswordService = resetForPaswordService;

    [Function("ResetForgotPasword")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var rfpr = await _resetForPaswordService.UnpackResetForgotPasswordRequest(req);
            if (rfpr != null)
            {
                var reset = await _resetForPaswordService.ResetPassword(rfpr);

                if(reset)
                {
                    return new OkResult();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ResetForgotPassword.Run() :: {ex.Message}");
        }
        return new BadRequestResult();
    }
}
