using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using MudExtensions.Services;
using GeoStream.Components;
using GeoStream.Data;
using GeoStream.Models;
using GeoStream.RabbitMQ.Bus;
using GeoStream.RabbitMQ.Contracts.Bus;
using GeoStream.RabbitMQ.Events;
using GeoStream.Services;
using GeoStream.UIEventsMediator;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Localization settings - Uncomment to configure the application to format dates and other culture-dependent data according to a specific
// culture, while ensuring that decimal numbers are formatted using the English (US) standard, with a dot (.) as the decimal separator.

//var customCulture = new CultureInfo("your-custom-culture") // e.g., "en-US", "es-ES", "fr-FR"
//{
//    NumberFormat = new CultureInfo("en-US").NumberFormat,
//    DateTimeFormat = new DateTimeFormatInfo
//    {
//        ShortDatePattern = "d/M/yyyy",
//        LongTimePattern = "HH:mm:ss"
//    }
//};

var customCulture = new CultureInfo("en-US");

CultureInfo.DefaultThreadCurrentCulture = customCulture;
CultureInfo.DefaultThreadCurrentUICulture = customCulture;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo> { customCulture };
    options.DefaultRequestCulture = new RequestCulture(customCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    //Ignores client's browser culture preference ('accept-language' header) effectively displaying dates in the specified format for customCulture
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CustomRequestCultureProvider(context => Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(customCulture.Name))));
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GeoStreamDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(
    options =>
    {
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<GeoStreamDbContext>().AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
});

//builder.Logging.AddFile("logs/geoStream-{Date}.txt");

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddSingleton<IApiClient, ApiClient>();
builder.Services.AddSingleton<IKmzConversionService, KmzConversionService>();

//Todo: Remove for production
builder.Services.AddHttpClient("BypassSSLClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });


#region RabbitMQ

builder.Services.AddTransient<IMediator>(serviceProvider =>
{
    var publisher = serviceProvider.GetRequiredService<INotificationPublisher>();
    return new Mediator(serviceProvider, publisher);
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddScoped<EmitterStoredEventHandler>();
builder.Services.AddScoped<ScannerConnectionStateEventHandler>();
builder.Services.AddSingleton<IEventAggregator, EventAggregator>();

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQSettings"));

builder.Services.AddSingleton<IEventBus, RabbitMQBus>(sc =>
{
    var scopeFactory = sc.GetRequiredService<IServiceScopeFactory>();
    var optionFactory = sc.GetRequiredService<IOptions<RabbitMQSettings>>();
    return new RabbitMQBus(scopeFactory, optionFactory);
});

#endregion

builder.Services.AddRazorPages();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMudServices();

builder.Services.AddMudExtensions();

builder.Services.AddControllers();

var app = builder.Build();

app.UseRequestLocalization();

#region RabbitMQ

var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Suscribe<EmitterStoredEvent, EmitterStoredEventHandler>();
eventBus.Suscribe<ScannerConnectionStateEvent, ScannerConnectionStateEventHandler>();

#endregion

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorPages();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GeoStreamDbContext>();
    dbContext.Database.EnsureCreated();
    dbContext.SeedData();
}

app.Run();
