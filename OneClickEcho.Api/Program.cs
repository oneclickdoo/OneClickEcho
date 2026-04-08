using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseForwardedHeaders();

    // Use the CORS policy
    app.UseCors("AllowNext");

    // Public media for Viber (must be reachable without auth; register before endpoint routing)
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "UploadedFiles")),
        RequestPath = "/uploads"
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
