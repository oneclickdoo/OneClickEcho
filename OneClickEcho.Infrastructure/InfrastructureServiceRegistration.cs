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
        services.Configure<OpenAiSettings>(configuration.GetSection("OpenAi"));

        // add Redis configuration
        string? redisConfiguration = configuration.GetSection("Redis")["ConnectionString"];

        if (string.IsNullOrEmpty(redisConfiguration))
        {
            throw new Exception("Redis:ConnectionString must be defined.");
        }

        ConfigurationOptions redisOptions = ConfigurationOptions.Parse(redisConfiguration);
        redisOptions.AbortOnConnectFail = false;

        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisOptions);

        // register Redis services
        services.AddSingleton<IConnectionMultiplexer>(redis);
        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        // register Scheduling services
        services.AddSchedulingService();

        // add Viber configuration
        ViberSettings viberSettings = new();

        configuration.GetSection("Viber").Bind(viberSettings);

        services.Configure<ViberSettings>(configuration.GetSection("Viber"));
        services.Configure<PublicUploadsSettings>(configuration.GetSection(PublicUploadsSettings.SectionName));

        bool viberConfigured =
            !string.IsNullOrWhiteSpace(viberSettings.Username) && !string.IsNullOrWhiteSpace(viberSettings.Password);

        services.AddHttpClient("ViberHttpClient", client =>
        {
            client.BaseAddress = new Uri("http://publicbulk.comtrade.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // Default ~100s can still allow long Polly retry chains; cap so a stuck Comtrade call cannot block sends for many minutes.
            client.Timeout = TimeSpan.FromSeconds(120);
        });

        if (viberConfigured)
        {
            services.AddScoped<IViberService, ViberService>();
        }
        else
        {
            services.AddScoped<IViberService, UnconfiguredViberService>();
        }

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
            // Do not set Host manually — it can break TLS/SNI or header handling; the host is taken from the request URI.
        });

        // register message services
        services.AddScoped<IStringTemplatingService, StringTemplatingService>();
        services.AddHttpClient<ISmsService, SmsService>(client =>
        {
            client.BaseAddress = new Uri("https://sms.oneclick.rs/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        services.AddScoped<IMessageSendingService, MessageSendingService>();
        services.AddScoped<IMessageDeliveryService, MessageDeliveryService>();

        // register other services (exclude Services.GptService: Scrutor would register IGptService/GptService and
        // IGptRequestStrategy/* — a duplicate IGptService->GptService descriptor is still validated and fails
        // without GptRequestStrategyFactory when OpenAI is off, or duplicates concrete strategy resolution)
        services
            .Scan(
                selector => selector
                    .FromAssemblies(
                        Assembly.GetExecutingAssembly())
                    .AddClasses(
                        classes => classes.Where(type =>
                            type.Namespace != "OneClickEcho.Infrastructure.Services.GptService"),
                        publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

        // OpenAI / GPT — registered only here
        OpenAiSettings openAiSettings = new();
        configuration.GetSection("OpenAi").Bind(openAiSettings);

        bool openAiConfigured =
            !string.IsNullOrWhiteSpace(openAiSettings.ApiKey) && !string.IsNullOrWhiteSpace(openAiSettings.Model);

        if (openAiConfigured)
        {
            string openAiBaseUrl = string.IsNullOrWhiteSpace(openAiSettings.BaseUrl)
                ? "https://api.openai.com/"
                : openAiSettings.BaseUrl.TrimEnd('/') + "/";

            services.AddHttpClient("OpenAiHttpClient", client =>
            {
                client.BaseAddress = new Uri(openAiBaseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiSettings.ApiKey}");
            });

            services.AddScoped<GenerateNewCampaignMessageStrategy>();
            services.AddScoped<EnhanceCampaignMessageStrategy>();
            services.AddScoped<ConvertNounCasesStrategy>();
            services.AddScoped<GptRequestStrategyFactory>();
            services.AddScoped<IGptService, GptService>();
        }
        else
        {
            services.AddScoped<IGptService, UnconfiguredOpenAiGptService>();
        }

        return services;
    }
}