using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Application.Common.Services.SmsService;
using OneClickEcho.Application.Common.Services.ViberService;
using OneClickEcho.Infrastructure.Services.DataManagement;
using OneClickEcho.Infrastructure.Services.GptService;
using OneClickEcho.Infrastructure.Services.MessageHandling;
using OneClickEcho.Infrastructure.Services.MessageHandling.Sms;
using OneClickEcho.Infrastructure.Services.MessageHandling.Viber;
using OneClickEcho.Infrastructure.Services.Scheduling;
using OneClickEcho.Infrastructure.Settings;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.Reflection;

namespace OneClickEcho.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        // add OpenAI configuration
        OpenAiSettings openAiSettings = new();

        configuration.GetSection("OpenAi").Bind(openAiSettings);

        if (string.IsNullOrEmpty(openAiSettings.ApiKey) || string.IsNullOrEmpty(openAiSettings.Model))
        {
            throw new Exception("OpenAi:ApiKey and OpenAi:Model must be defined.");
        }

        services.AddHttpClient("OpenAiHttpClient", client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiSettings.ApiKey}");
        });

        services.Configure<OpenAiSettings>(configuration.GetSection("OpenAi"));

        // register OpenAI services
        services.AddScoped<GenerateNewCampaignMessageStrategy>();
        services.AddScoped<EnhanceCampaignMessageStrategy>();
        services.AddScoped<ConvertNounCasesStrategy>();
        services.AddScoped<GptRequestStrategyFactory>();
        services.AddScoped<IGptService, GptService>();

        // add Redis configuration
        string? redisConfiguration = configuration.GetSection("Redis")["ConnectionString"];

        if (string.IsNullOrEmpty(redisConfiguration))
        {
            throw new Exception("Redis:ConnectionString must be defined.");
        }

        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConfiguration);

        // register Redis services
        services.AddSingleton<IConnectionMultiplexer>(redis);
        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        // register Scheduling services
        services.AddSchedulingService();

        // add Viber configuration
        ViberSettings viberSettings = new();

        configuration.GetSection("Viber").Bind(viberSettings);

        if (string.IsNullOrEmpty(viberSettings.Username) || string.IsNullOrEmpty(viberSettings.Password))
        {
            throw new Exception("Viber:Username and Viber:Password must be defined.");
        }

        services.AddHttpClient("ViberHttpClient", client =>
        {
            client.BaseAddress = new Uri("http://publicbulk.comtrade.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("Host", "publicbulk.comtrade.com");
        });

        services.Configure<ViberSettings>(configuration.GetSection("Viber"));

        // add SMS configuration
        /*SMSSettings smsSettings = new();

        configuration.GetSection("SMS").Bind(smsSettings);

        if (string.IsNullOrEmpty(smsSettings.Username) || string.IsNullOrEmpty(smsSettings.Password))
        {
            throw new Exception("SMS:Username and SMS:Password must be defined.");
        }*/

        services.AddHttpClient("SmsHttpClient", client =>
        {
            client.BaseAddress = new Uri("https://sms.oneclick.rs/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("username", smsSettings.Username);
            //client.DefaultRequestHeaders.Add("pwd", smsSettings.Password);
            client.DefaultRequestHeaders.Add("Host", "sms.oneclick.rs");
        });

        // register message services
        services.AddScoped<IStringTemplatingService, StringTemplatingService>();
        services.AddScoped<IViberService, ViberService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IMessageSendingService, MessageSendingService>();
        services.AddScoped<IMessageDeliveryService, MessageDeliveryService>();

        // register other services
        services
            .Scan(
                selector => selector
                    .FromAssemblies(
                        Assembly.GetExecutingAssembly())
                    .AddClasses(false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

        return services;
    }
}