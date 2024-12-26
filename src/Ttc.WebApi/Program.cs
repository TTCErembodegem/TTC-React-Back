using CoreWCF.Configuration;
using CoreWCF.Description;
using Serilog;
using System.Text.Json.Serialization;
using Ttc.DataAccess;
using Ttc.Model.Core;
using Ttc.WebApi.Emailing;
using Ttc.WebApi.Utilities;
using Ttc.WebApi.Utilities.Auth;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
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
    builder.Services.AddScoped<TtcLogger>();
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

    builder.Services.AddServiceModelServices().AddServiceModelMetadata();
    builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    //var myWSHttpBinding = new WSHttpBinding(SecurityMode.Transport);
    //myWSHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

    //app.UseServiceModel(builder =>
    //{
    //    builder.AddService<EchoService>((serviceOptions) => { })
    //        // Add a BasicHttpBinding at a specific endpoint
    //        .AddServiceEndpoint<EchoService, IEchoService>(new BasicHttpBinding(), "/EchoService/basichttp")
    //        // Add a WSHttpBinding with Transport Security for TLS
    //        .AddServiceEndpoint<EchoService, IEchoService>(myWSHttpBinding, "/EchoService/WSHttps");
    //});

    var serviceMetadataBehavior = app.Services.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;

    app.UseCors();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
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
