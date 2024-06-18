using System.Diagnostics;
using System.Reflection;
using MassTransit;
using MassTransit.Metadata;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sample.Api;
using Sample.Components;
using Serilog;
using Serilog.Events;

const string Prefix = "SampleEntityLiteOutboxSaga";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Sample", LogEventLevel.Debug)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddScoped<IRegistrationService, RegistrationService>();

builder.Services.AddControllers();

builder.Services.AddScoped<RegistrationDataService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Default");
    if (connectionString == null)
    {
        throw new InvalidOperationException("Connection string 'Default' is missing.");
    }
    return new RegistrationDataService(connectionString, "System.Data.SqlClient");
});


builder.Services.AddOpenTelemetry().WithTracing(x =>
{
    x.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("api")
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector())
        .AddSource("MassTransit")
        .AddAspNetCoreInstrumentation()
        .AddJaegerExporter(o =>
        {
            o.AgentHost = HostMetadataCache.IsRunningInContainer ? "jaeger" : "localhost";
            o.AgentPort = 6831;
            o.MaxPayloadSizeInBytes = 4096;
            o.ExportProcessorType = ExportProcessorType.Batch;
            o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
            {
                MaxQueueSize = 2048,
                ScheduledDelayMilliseconds = 5000,
                ExporterTimeoutMilliseconds = 30000,
                MaxExportBatchSize = 512,
            };
        });
});
builder.Services.AddMassTransit(x =>
{
    x.AddEntityLiteOutbox<RegistrationDataService>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UseSqlServer();
        o.UseBusOutbox();
        o.DisableInboxCleanupService();
    });

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(Prefix, true));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.MessageTopology.SetEntityNameFormatter(new PrefixEntityNameFormatter(cfg.MessageTopology.EntityNameFormatter, Prefix + ":"));
        cfg.ConfigureEndpoints(context);
        cfg.AutoStart = true;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
