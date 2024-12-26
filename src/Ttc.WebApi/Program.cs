using CoreWCF.Configuration;
using CoreWCF.Description;
using Serilog;
using Serilog.Context;
using System.Text.Json.Serialization;
using Ttc.DataAccess;
using Ttc.Model.Core;
using Ttc.WebApi.Emailing;
using Ttc.WebApi.Utilities;
using Ttc.WebApi.Utilities.Auth;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message} {Properties}{NewLine}{Exception}")
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message} {Properties}{NewLine}{Exception}")
    .CreateLogger();

Log.Information("Starting up...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(b => {
            b.AllowAnyOrigin();
            b.AllowAnyMethod();
            b.AllowAnyHeader();
        });
    });
    builder.Services.AddSerilog(Log.Logger);
    builder.Services.AddSingleton<TtcLogger>();
    builder.Services.AddScoped<EmailService>();
    builder.Services.AddScoped<UserProvider>();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    builder.Services.AddEndpointsApiExplorer();
    AddSwagger.Configure(builder.Services);
    var ttcSettings = LoadSettings.Configure(builder.Services);
    GlobalBackendConfiguration.Configure(builder.Services, ttcSettings);
    AddAuthentication.Configure(builder.Services, ttcSettings);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddServiceModelServices().AddServiceModelMetadata();
    builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    var serviceMetadataBehavior = app.Services.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;

    app.UseCors();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.Use(async (context, next) =>
    {
        LogContext.PushProperty("UserName", context.User.Identity?.Name ?? "Anonymous");
        await next();
    });
    app.UseSerilogRequestLogging();
    app.MapControllers();
    app.UseExceptionHandler();
    app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "Something went wrong");
}
finally
{
    await Log.CloseAndFlushAsync();
}
