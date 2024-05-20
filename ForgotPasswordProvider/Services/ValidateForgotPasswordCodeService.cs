using ForgotPasswordProvider.Data.Contexts;
using ForgotPasswordProvider.Functions;
using ForgotPasswordProvider.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForgotPasswordProvider.Services;

public class ValidateForgotPasswordCodeService(ILogger<ValidateForgotPasswordCodeService> logger, DataContext context) : IValidateForgotPasswordCodeService
{
    private readonly ILogger<ValidateForgotPasswordCodeService> _logger = logger;
    private readonly DataContext _context = context;

    public async Task<ValidateRequest> UnpackValidateRequestAsync(HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            if (!string.IsNullOrEmpty(body))
            {
                var validateRequest = JsonConvert.DeserializeObject<ValidateRequest>(body);

                if (validateRequest != null)
                {
                    return validateRequest;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateForgotPasswordCode.UnpackValidateRequestAsync() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> ValidateCodeAsync(ValidateRequest request)
    {
        try
        {
            var entity = await _context.ForgotPasswordRequests.FirstOrDefaultAsync(x => x.Email == request.Email && x.Code == request.Code);

            if (entity != null)
            {
                _context.ForgotPasswordRequests.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateForgotPasswordCode.ValidateCodeAsync() :: {ex.Message}");
        }
        return false;
    }
}
