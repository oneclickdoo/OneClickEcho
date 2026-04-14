using System.Globalization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using OneClickEcho.App.Infrastructure;
using OneClickEcho.App.Infrastructure.Middleware;
using OneClickEcho.Application;
using OneClickEcho.Infrastructure;
using OneClickEcho.Persistence;
using OneClickEcho.Seeder;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog((context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration));

    builder.Configuration.AddEnvironmentVariables();

    // Shorter env alias (Docker/hosting): VIBER_CAMPAIGN_MESSAGE_ID_FLOOR=50000000 — root .env is not auto-injected into the API container unless compose maps it.
    string? simpleFloor = Environment.GetEnvironmentVariable("VIBER_CAMPAIGN_MESSAGE_ID_FLOOR");
    if (!string.IsNullOrWhiteSpace(simpleFloor)
        && long.TryParse(simpleFloor, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsedFloor)
        && parsedFloor >= 0)
    {
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?> { ["Messaging:CampaignLeadViberMessageId:Floor"] = parsedFloor.ToString(CultureInfo.InvariantCulture) });
    }

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add CORS policy
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowNext",
            policy => policy.WithOrigins(
                                         "http://localhost:3000",
                                         "https://app.localhost8080.xyz",
                                         "https://viber.oneclick.rs",
                                         "http://viber.oneclick.rs")
                                         .AllowAnyMethod()
                                         .AllowCredentials()
                                         .AllowAnyHeader());
    });

    builder.Services.AddInfrastructureService(builder.Configuration);
    builder.Services.AddPersistenceService(builder.Configuration, builder.Environment);
    builder.Services.AddApplicationService();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddProblemDetails();

    builder.Services.AddControllers();
    
    builder.Services.Configure<FormOptions>(options =>
    {
        options.ValueLengthLimit = int.MaxValue;
        options.MultipartBodyLengthLimit = int.MaxValue;
        options.MultipartHeadersLengthLimit = int.MaxValue;
    });
    
    builder.Services.Configure<KestrelServerOptions>(options =>
    {
        options.Limits.MaxRequestBodySize = int.MaxValue;
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

WebApplication app = builder.Build();
{
    Log.Information(
        "Viber message id floor: resolved={Resolved}, env Messaging__CampaignLeadViberMessageId__Floor={LongEnv}, env VIBER_CAMPAIGN_MESSAGE_ID_FLOOR={ShortEnv}",
        app.Configuration.GetValue<long>("Messaging:CampaignLeadViberMessageId:Floor"),
        Environment.GetEnvironmentVariable("Messaging__CampaignLeadViberMessageId__Floor") ?? "(unset)",
        Environment.GetEnvironmentVariable("VIBER_CAMPAIGN_MESSAGE_ID_FLOOR") ?? "(unset)");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseForwardedHeaders();

    // Use the CORS policy
    app.UseCors("AllowNext");

    // Public media for Viber (must be reachable without auth; explicit MIME helps Comtrade accept video/image — substatus 28 otherwise)
    var uploadsContentTypes = new FileExtensionContentTypeProvider();
    uploadsContentTypes.Mappings[".mp4"] = "video/mp4";
    uploadsContentTypes.Mappings[".jpg"] = "image/jpeg";
    uploadsContentTypes.Mappings[".jpeg"] = "image/jpeg";
    uploadsContentTypes.Mappings[".png"] = "image/png";
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "UploadedFiles")),
        RequestPath = "/uploads",
        ContentTypeProvider = uploadsContentTypes,
        OnPrepareResponse = ctx =>
        {
            // Allow Comtrade/Viber fetchers to read length/range where applicable
            ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
        }
    });

    // @TODO: Remove if not needed
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapDefaultControllerRoute();
    app.UseHttpsRedirection();
    app.UseMiddleware<RequestLogContextMiddleware>();
    app.UseCertificateForwarding();
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
}

await SeederRunner.Execute(app.Services);

app.Run();
