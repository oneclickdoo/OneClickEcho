using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using OneClickEcho.App.Infrastructure;
using OneClickEcho.App.Infrastructure.Middleware;
using OneClickEcho.Application;
using OneClickEcho.Infrastructure;
using OneClickEcho.Persistence;
using OneClickEcho.Seeder;
using Serilog;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog((context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration));

    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "OneClickEcho API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Unesi: Bearer {token}"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });


    // Add CORS policy
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowNext",
            policy => policy.WithOrigins("http://localhost:3000", "https://app.localhost8080.xyz")
                                         .AllowAnyMethod()
                                         .AllowCredentials()
                                         .AllowAnyHeader());
    });

    builder.Services.AddInfrastructureService(builder.Configuration);
    builder.Services.AddPersistenceService(builder.Configuration);
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
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

WebApplication app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Use the CORS policy
    app.UseCors("AllowNext");

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

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "UploadedFiles")),
        RequestPath = "/uploads"
    });
}

await SeederRunner.Execute(app.Services);

app.Run();
