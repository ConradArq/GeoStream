using MediatR;
using Microsoft.Extensions.Options;
using System.Reflection;
using MessageProcessorService.Infrastructure.Logging;
using MessageProcessorService.Infrastructure.RabbitMQ;
using MessageProcessorService.Core.Interfaces;
using MessageProcessorService.Application.CommandHandlers;
using MessageProcessorService.Application.Commands;
using MessageProcessorService.Infrastructure.Persistence.MSSQL;
using Microsoft.EntityFrameworkCore;
using MessageProcessorService.Infrastructure.Persistence.MongoDB;
using MessageProcessorService.Domain.Events;
using MessageProcessorService.Application.EventHandlers;
using MessageProcessorService.Domain.Interfaces;
using MessageProcessorService.Infrastructure.Persistence.MongoDB.Respositories;
using MessageProcessorService.Infrastructure.Persistence.MSSQL.Repositories;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "MessageProcessorService";
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<GeoStreamDbContext>(options =>
        {
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddHttpClient();
        services.Configure<RabbitMQSettings>(hostContext.Configuration.GetSection("RabbitMQSettings"));
        services.Configure<MongoDBSettings>(hostContext.Configuration.GetSection("MongoDBSettings"));
        services.AddTransient<IEventHandler<EmitterReadEvent>, EmitterReadEventHandler>();
        services.AddTransient<IRequestHandler<EmitterStoredCommand, bool>, EmitterStoredCommandHandler>();
        services.AddTransient<IEmitterRepository, EmitterRepository>();
        services.AddTransient<IGeoStreamRepository, GeoStreamRepository>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddSingleton<IEventBus, RabbitMQEventBus>(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            var optionFactory = sp.GetRequiredService<IOptions<RabbitMQSettings>>();
            var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            return new RabbitMQEventBus(mediator, optionFactory, logger, scopeFactory);
        });
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.ClearProviders();
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MessageProcessorService.log");
        logging.AddProvider(new FileLoggerProvider(logPath, TimeZoneInfo.Local));
    });

var host = builder.Build();

// Subscribe to the event after the host is built
var eventBus = host.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<EmitterReadEvent, EmitterReadEventHandler>();

host.Run();