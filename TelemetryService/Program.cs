using MediatR;
using Microsoft.Extensions.Options;
using System.Reflection;
using TelemetryService.Application.CommandHandlers;
using TelemetryService.Application.Commands;
using TelemetryService.Infrastructure.Logging;
using TelemetryService.Infrastructure;
using TelemetryService.Infrastructure.RabbitMQ;
using TelemetryService.Application.Interfaces;
using TelemetryService.Application.Services;
using TelemetryService.Core.Interfaces;
using TelemetryService.Infrastructure.Scanner;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "TelemetryService";
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient();
        services.AddHostedService<Worker>();
        services.Configure<ScannerSettings>(hostContext.Configuration.GetSection("ScannerSettings"));
        services.Configure<RabbitMQSettings>(hostContext.Configuration.GetSection("RabbitMQSettings"));       
        services.AddTransient<IScannerReaderService, ScannerService>();
        services.AddTransient<IRequestHandler<EmitterReadCommand, bool>, EmitterReadCommandHandler>();
        services.AddTransient<IRequestHandler<ScannerConnectionStateCommand, bool>, ScannerConnectionStateCommandHandler>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddSingleton<IEventBus, RabbitMQEventBus>(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            var optionFactory = sp.GetRequiredService<IOptions<RabbitMQSettings>>();
            var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
            return new RabbitMQEventBus(mediator, optionFactory, logger);
        });
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.ClearProviders();
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TelemetryService.log");
        logging.AddProvider(new FileLoggerProvider(logPath, TimeZoneInfo.Local));
    });

var host = builder.Build();
host.Run();