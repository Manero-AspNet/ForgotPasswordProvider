using ForgotPasswordProvider.Data.Contexts;
using ForgotPasswordProvider.Models;
using ForgotPasswordProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForgotPasswordProvider.Functions;

public class ValidateForgotPasswordCode(ILogger<ValidateForgotPasswordCode> logger, IValidateForgotPasswordCodeService validateForgotPasswordCodeService)
{
    private readonly ILogger<ValidateForgotPasswordCode> _logger = logger;
    private readonly IValidateForgotPasswordCodeService _validateForgotPasswordCodeService = validateForgotPasswordCodeService;

    [Function("ValidateForgotPasswordCode")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var validateRequest = await _validateForgotPasswordCodeService.UnpackValidateRequestAsync(req);

            if(validateRequest != null)
            {
                var validateResult = await _validateForgotPasswordCodeService.ValidateCodeAsync(validateRequest);

                if (validateResult)
                {
                    return new OkResult();
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateForgotPasswordCode.Run() :: {ex.Message}");
        }
        return new BadRequestResult();
    }

    
}
