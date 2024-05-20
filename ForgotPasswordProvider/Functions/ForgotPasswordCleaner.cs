using System;
using ForgotPasswordProvider.Data.Contexts;
using ForgotPasswordProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForgotPasswordProvider.Functions;

public class ForgotPasswordCleaner(ILoggerFactory loggerFactory, IForgotPasswordCleanerService forgotPasswordCleanerService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ForgotPasswordCleaner>();
    private readonly IForgotPasswordCleanerService _forgotPasswordCleanerService = forgotPasswordCleanerService;

    [Function("ForgotPasswordCleaner")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        try
        {
            await _forgotPasswordCleanerService.RemoveExpiredRecordsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordCleaner.Run() :: {ex.Message}");
        }
    }

    
}
