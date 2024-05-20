using ForgotPasswordProvider.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForgotPasswordProvider.Services;

public class ForgotPasswordCleanerService(ILogger<ForgotPasswordCleanerService> logger, DataContext context) : IForgotPasswordCleanerService
{
    private readonly ILogger<ForgotPasswordCleanerService> _logger = logger;
    private readonly DataContext _context = context;

    public async Task RemoveExpiredRecordsAsync()
    {
        try
        {
            var expired = await _context.ForgotPasswordRequests.Where(x => x.ExpirationDate >= DateTime.Now).ToListAsync();
            _context.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordCleanerService.RemoveExpiredRecordsAsync() :: {ex.Message}");
        }
    }
}
