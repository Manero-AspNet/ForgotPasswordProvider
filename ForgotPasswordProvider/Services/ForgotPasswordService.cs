using ForgotPasswordProvider.Data.Contexts;
using ForgotPasswordProvider.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForgotPasswordProvider.Services;

public class ForgotPasswordService(ILogger<ForgotPasswordService> logger, DataContext context) : IForgotPasswordService
{
    private readonly ILogger<ForgotPasswordService> _logger = logger;
    private readonly DataContext _context = context;

    public async Task<ForgotPasswordRequest> UnpackForgotPasswordRequest(HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var forgotPasswordRequest = JsonConvert.DeserializeObject<ForgotPasswordRequest>(body);
            if (forgotPasswordRequest != null)
            {
                return forgotPasswordRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordService.UnpackForgotPasswordRequest() :: {ex.Message}");
        }
        return null!;
    }

    public string GeneratedCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(10000, 99999);

            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordService.GeneratedCode() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> SaveForgotPasswordRequest(string email, string code)
    {
        try
        {
            var existingRequest = await _context.ForgotPasswordRequests.FirstOrDefaultAsync(x => x.Email == email);

            if (existingRequest != null)
            {
                existingRequest.Code = code;
                existingRequest.ExpirationDate = DateTime.Now.AddMinutes(5);
                _context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                _context.ForgotPasswordRequests.Add(new() { Email = email, Code = code });
            }
            await _context.SaveChangesAsync();
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordService.SaveForgotPasswordRequest() :: {ex.Message}");
        }
        return false;
    }

    public EmailRequest GenerateEmailRequest(string email, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequest
                {
                    To = email,
                    Subject = $"Forgot Password {code}",
                    Body = $@"
                    <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Verification Code</title>
                        </head>
                        <body>
                            <div style='color: #191919; max-width: 500px'>
                                <div style='background-color: #4F85F6; color: white; text-align: center; padding: 20px 0;'>
                                    <h1 style='font-weight: 400;'>Verification Code</h1>
                                </div>
                                <div style='background-color: #f4f4f4; padding: 1rem 2rem;'>
                                    <p>Dear user,</p>
                                    <p>We received a request to sign in to your account using e-mail {email}. Please verify your account using this verification code:</p>
                                    <p class='code' style='font-weight: 700; text-align:center; font-size: 48px; letter-spacing: 8px;'>
                                        {code}
                                    </p>
                                    <div class='noreply' style='color: #191919; font-size: 11px;'>
                                        <p>If you did not request this code, it is possible that someone else is trying to access the Silicon Account <span style='color: #0041cd;'>{email}</span>. This email can't receive replies. For more information, visit the Silicons Help Center.</p>
                                    </div>
                                </div>
                                <div style='color: #191919; text-align:center; font-size: 11px;'>
                                    <p>© Silicon, Sveavägen 1, SE-123 45 Stockholm, Sweden</p>
                                </div> 
                            </div>
                        </body>
                    </html>",
                    PlainText = $"Please verify your account using this verification code: {code}. If you did not request this code, it is possible that someone else is trying to access the Silicon Account {email}. This email can't receive replies. For more information, visit the Silicons Help Center. © Silicon, Sveavägen 1, SE-123 45 Stockholm, Sweden"
                };
                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordService.GenerateEmailRequest() :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateServiceBusMessage(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);

            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPasswordService.GenerateServiceBusMessage() :: {ex.Message}");
        }
        return null!;
    }
}
