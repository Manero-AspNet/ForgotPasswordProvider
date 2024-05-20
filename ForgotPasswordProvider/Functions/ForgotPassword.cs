using Azure.Messaging.ServiceBus;
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

public class ForgotPassword(ILogger<ForgotPassword> logger, ServiceBusClient serviceBusClient, IForgotPasswordService forgotPasswordService)
{
    private readonly ILogger<ForgotPassword> _logger = logger;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
    private readonly IForgotPasswordService _forgotPasswordService = forgotPasswordService;

    [Function("ForgotPassword")]
    [ServiceBusOutput("email_request", Connection="ServiceBus")]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        try
        {
            var fpr = await _forgotPasswordService.UnpackForgotPasswordRequest(req);
            if (fpr != null)
            {
                var code = _forgotPasswordService.GeneratedCode();

                if (!string.IsNullOrEmpty(code))
                {
                    var result = await _forgotPasswordService.SaveForgotPasswordRequest(fpr.Email, code);

                    if (result)
                    {
                        var emailRequest = _forgotPasswordService.GenerateEmailRequest(fpr.Email, code);

                        if(emailRequest != null)
                        {
                            var payload = _forgotPasswordService.GenerateServiceBusMessage(emailRequest);

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
}
