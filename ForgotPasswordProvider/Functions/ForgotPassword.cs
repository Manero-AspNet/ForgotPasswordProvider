using Azure.Messaging.ServiceBus;
using ForgotPasswordProvider.Data.Contexts;
using ForgotPasswordProvider.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForgotPasswordProvider.Functions;

public class ForgotPassword
{
    private readonly ILogger<ForgotPassword> _logger;
    private readonly DataContext _context;
    private readonly ServiceBusClient _serviceBusClient;

    public ForgotPassword(ILogger<ForgotPassword> logger, DataContext context, ServiceBusClient serviceBusClient)
    {
        _logger = logger;
        _context = context;
        _serviceBusClient = serviceBusClient;
    }

    [Function("ForgotPassword")]
    [ServiceBusOutput("email_request", Connection="ServiceBus")]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        try
        {
            var fpr = await UnpackForgotPasswordRequest(req);
            if (fpr != null)
            {
                var code = GeneratedCode();

                if (!string.IsNullOrEmpty(code))
                {
                    var result = await SaveForgotPasswordRequest(fpr.Email, code);

                    if (result)
                    {
                        var emailRequest = GenerateEmailRequest(fpr.Email, code);

                        if(emailRequest != null)
                        {
                            var payload = GenerateServiceBusMessage(emailRequest);

                            if (!string.IsNullOrEmpty(payload))
                            {
                                var sender = _serviceBusClient.CreateSender("email_request");
                                await sender.SendMessageAsync(new ServiceBusMessage(payload)
                                {
                                    ContentType = "application/json"
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ForgotPassword.Run() :: {ex.Message}");
        }
    }

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
            _logger.LogError($"ERROR : ForgotPassword.UnpackForgotPasswordRequest() :: {ex.Message}");
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
            _logger.LogError($"ERROR : ForgotPassword.GeneratedCode() :: {ex.Message}");
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
            _logger.LogError($"ERROR : ForgotPassword.SaveForgotPasswordRequest() :: {ex.Message}");
        }
        return false;
    }

    public EmailRequest GenerateEmailRequest(string email, string code)
    {
        try
        {
            if(!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(code))
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
            _logger.LogError($"ERROR : ForgotPassword.GenerateEmailRequest() :: {ex.Message}");
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
            _logger.LogError($"ERROR : ForgotPassword.GenerateServiceBusMessage() :: {ex.Message}");
        }
        return null!;
    }
}
