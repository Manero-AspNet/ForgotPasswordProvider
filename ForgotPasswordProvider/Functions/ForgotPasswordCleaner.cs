using System;
using ForgotPasswordProvider.Data.Contexts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForgotPasswordProvider.Functions;

public class ForgotPasswordCleaner
{
    private readonly ILogger _logger;
    private readonly DataContext _context;

    public ForgotPasswordCleaner(ILoggerFactory loggerFactory, DataContext context)
    {
        _logger = loggerFactory.CreateLogger<ForgotPasswordCleaner>();
        _context = context;
    }

    [Function("ForgotPasswordCleaner")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        try
        {
            await RemoveExpiredRecordsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordCleaner.Run() :: {ex.Message}");
        }
    }

    public async Task RemoveExpiredRecordsAsync()
    {
        try
        {
            var expired = await _context.ForgotPasswordRequests.Where(x => x.ExpirationDate <= DateTime.Now).ToListAsync();
            _context.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordCleaner.RemoveExpiredRecordsAsync() :: {ex.Message}");
        }
    }
}
