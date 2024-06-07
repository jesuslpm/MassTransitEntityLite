using System.Data;
using System.Diagnostics;
using MassTransit;
using MassTransit.Metadata;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sample.Components;
using Sample.Components.Consumers;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddScoped<RegistrationDataService>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Default");
            if (connectionString == null)
            {
                throw new InvalidOperationException("Connection string 'Default' is missing.");
            }
            return new RegistrationDataService(connectionString, "System.Data.SqlClient");
        });

        services.AddOpenTelemetry().WithTracing(x =>
        {
            x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("service")
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .AddSource("MassTransit")
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

        services.AddMassTransit(x =>
        {
            x.AddEntityLiteOutbox<RegistrationDataService>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(30);
                o.IsolationLevel = IsolationLevel.ReadCommitted;
                o.UseSqlServer();
                o.UseBusOutbox(cfg =>
                {
                    cfg.DisableDeliveryService();
                });
            });

            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumer<NotifyRegistrationConsumer>();
            x.AddConsumer<RegistrationConsumer, RegistrationConsumerDefinition>();
            x.AddConsumer<SendRegistrationEmailConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .UseSerilog()
    .Build();

await host.RunAsync();