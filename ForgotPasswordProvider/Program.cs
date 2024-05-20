using Azure.Messaging.ServiceBus;
using ForgotPasswordProvider.Data.Contexts;
using ForgotPasswordProvider.Data.Entities;
using ForgotPasswordProvider.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<DataContext>(x => x.UseSqlServer(Environment.GetEnvironmentVariable("SqlServer")));
        services.AddSingleton<ServiceBusClient>(new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBus")));
        services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
        services.AddScoped<IForgotPasswordCleanerService, ForgotPasswordCleanerService>();
        services.AddScoped<IValidateForgotPasswordCodeService, ValidateForgotPasswordCodeService>();
        services.AddScoped<IResetForgotPasswordService, ResetForgotPasswordService>();

        services.AddIdentity<UserEntitiy, IdentityRole>(x =>
        {
            x.User.RequireUniqueEmail = true;
            x.SignIn.RequireConfirmedEmail = true;
            x.Password.RequiredLength = 8;
        }).AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();
    })
    .Build();

host.Run();
