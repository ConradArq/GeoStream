using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using GeoStream.Api.API.Filters;
using GeoStream.Api.API.Middlewares;
using GeoStream.Api.API.ModelBinders.Providers;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Application.Mappings;
using GeoStream.Api.Application.Services;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Infrastructure.Interfaces.Logging;
using GeoStream.Api.Infrastructure.Interfaces.Providers;
using GeoStream.Api.Infrastructure.Interfaces.Services;
using GeoStream.Api.Infrastructure.Logging;
using GeoStream.Api.Infrastructure.Persistence.MongoDB;
using GeoStream.Api.Infrastructure.Persistence.MongoDB.Respositories;
using GeoStream.Api.Infrastructure.Persistence.MSSQL;
using GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories;
using GeoStream.Api.Infrastructure.Providers;
using GeoStream.Api.Infrastructure.Services;
using GeoStream.Api.Infrastructure.Services.BackgroundServices;
using GeoStream.Api.Infrastructure.Services.Queues;
using GeoStream.Api.Infrastructure.Settings;
using ValidationException = GeoStream.Api.Application.Exceptions.ValidationException;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region Services

#region API
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers(options =>
{
    // The ModelBinderProviders are called in the order in which they are registered. For each provider, the framework calls
    // the GetBinder method to determine if the provider can supply a binder for the current model type and binding source.
    // Once a provider returns a binder, that binder is used to bind the model and the pipeline stops processing other
    // providers. If the binder cannot bind the model correctly, an error is raised, and the pipeline bypasses the rest.
    options.Filters.Add<CustomActionFilter>();
    options.ModelBinderProviders.Insert(0, new RouteParameterBinderProvider());
    options.ModelBinderProviders.Insert(1, new QueryStringBinderProvider());
    options.ModelBinderProviders.Insert(2, new JsonBinderProvider());
    // TODO: Implement FormDataBinderProvider to provide localized error messages for form data binding errors.
    // NOTE:
    // - The FormDataBinderProvider is responsible for handling models with BindingSource.Form.
    // - In general, BindingSource.Form applies to the entire form object.
    // - After the form object is bound, each property of the form model is passed through the binding pipeline.
    // - To localize or customize error messages for each individual property, custom binders for the corresponding
    //   property types (e.g., string, int, custom objects) would need to be implemented.
})
.ConfigureApiBehaviorOptions(options =>
{
    // Uncomment to disable automatic 400 Bad Request responses for invalid models. If SuppressModelStateInvalidFilter is
    // set to true, InvalidModelStateResponseFactory will not handle the response, so manual checking of ModelState.IsValid 
    // would be required if no other validation mechanism is in place.
    //// options.SuppressModelStateInvalidFilter = true;

    options.InvalidModelStateResponseFactory = context =>
    {
        // This runs when validation errors occur. Remove errors from ModelState with key "requestDto". The System.Text.Json
        // serializer adds these messages when it fails to bind JSON to the model parameter (in this project it is always
        // called "requestDto"). These errors are often redundant or unclear, so they are excluded for cleaner responses.
        var validationErrors = context.ModelState
            .Where(x => x.Value != null && x.Key != "requestDto" && x.Value.Errors.Count > 0)
            .ToDictionary(x => x.Key, x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray())
            .Where(x => x.Value.Length > 0)
            .ToDictionary(x => x.Key, x => x.Value);

        // If validationErrors is empty, add a custom error about the request body being null.
        if (!validationErrors.Any())
        {
            validationErrors["requestDto"] = new[] { "The body of the request cannot be null or empty." };
        }

        throw new ValidationException(validationErrors);
    };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    // Browsers block AllowAnyOrigin() when credentials like Authorization headers are sent, so origins in the CORS policy are used instead.
    var origins = builder.Configuration.GetSection("Cors:ClientAppOrigins").Get<string[]>()
        ?? throw new InvalidOperationException("CORS origin is not configured. Please set 'Cors:ClientAppOrigin' in configuration.");
    options.AddPolicy("ClientApp", builder =>
    {
        builder.WithOrigins(origins)
               .AllowCredentials()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {

        Title = "GeoStream.Api",
        Version = "v1",
        Description = "This API provides CRUD operations for managing asset insights data.",
        Contact = new OpenApiContact()
        {
            Name = "Development by: conra.arq@gmail.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please insert the JWT token in this format: Bearer {your token here}"
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
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),
        // Uncomment to allow tokens without an exp claim while validating those with exp claim (for testing purposes, not production)
        ////LifetimeValidator = (notBefore, expires, token, parameters) =>
        ////{
        ////    if (!expires.HasValue)
        ////    {
        ////        return true;
        ////    }
        ////    return DateTime.UtcNow < expires.Value;
        ////}
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EntityOwnershipPolicy", policy =>
    {
        policy.Requirements.Add(new EntityOwnershipRequirement());
        // Optionally add a specific entity type and ID parameter for customization.
        // Use this if the entity name does not match the controller name or the ID parameter is not "id".
        //policy.Requirements.Add(new EntityOwnershipRequirement(entityType: typeof(Entity2), idParameterName: "customId"));
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, EntityOwnershipHandler>();

#endregion

#region Application

builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IScannerService, ScannerService>();
builder.Services.AddScoped<IHubService, HubService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<IAssetRegistryService, AssetRegistryService>();
builder.Services.AddScoped<IEmitterService, EmitterService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// FluentValidation scans the specified assembly for classes implementing AbstractValidator<T> using reflection and
// automatically registers them in the DI container. `AddValidatorsFromAssemblyContaining<T>()` can also be used to
// specify a type that resides within the assembly instead of explicitly referencing the assembly.
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters().AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

QueryProfile.InitializeMappings();

#endregion

#region Infrastructure

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IRouteHubRepository, RouteHubRepository>();
builder.Services.AddScoped<IScannerRepository, ScannerRepository>();
builder.Services.AddScoped<IHubRepository, HubRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IAssetEmitterRepository, AssetEmitterRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IEmitterRepository, EmitterRepository>();

builder.Services.AddSingleton<IHttpService, HttpService>();

builder.Services.AddDbContext<GeoStreamDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
    //*,sqlServerOptions => sqlServerOptions.CommandTimeout(60)*/)
});

// Configure HostOptions to handle unhandled exceptions in BackgroundService
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

#endregion

#region Logging

builder.Logging.ClearProviders();

builder.Logging.AddConsole();

builder.Services.AddSingleton<ILogQueueService, LogQueueService>();

builder.Services.AddSingleton<ILoggerProvider, ApiLoggerProvider>();

builder.Services.AddSingleton<IApiLogger>(serviceProvider =>
{
    var logQueueService = serviceProvider.GetRequiredService<ILogQueueService>();
    return new ApiLogger("DefaultCategory", logQueueService, serviceProvider.GetService<IHttpContextAccessor>());
});

builder.Services.AddHostedService<LogBackgroundService>();

builder.Services.Configure<LoggerFilterOptions>(options =>
{
    // Prevent logs produced by the LogBackgroundService from being written to the ApiLoggerProvider.
    // This avoids potential feedback loops where the logger writes logs about its own logging process.
    options.Rules.Add(new LoggerFilterRule(
        providerName: typeof(ApiLoggerProvider).FullName,
        categoryName: typeof(LogBackgroundService).FullName,
        logLevel: LogLevel.None,
        filter: null
    ));

    // Allow application-level logs at Information level or higher in the custom provider. 
    // This applies to logs categorized under "ProjectBase.*".
    options.Rules.Add(new LoggerFilterRule(
        providerName: typeof(ApiLoggerProvider).FullName,
        categoryName: $"{typeof(ApiLoggerProvider).Namespace?.Split('.')[0] ?? "DefaultNamespace"}.*",
        logLevel: LogLevel.Information,
        filter: null
        ));

    // Restrict logs from .NET or other libraries to Error level or higher in the custom provider.
    // This applies to all categories and ensures that only critical errors from third-party libraries or .NET internals are logged.
    // It is mutually exclusive from the rule allowing application-level logs at Information level or higher, 
    // ensuring that logs from the application and external libraries are handled separately.
    options.Rules.Add(new LoggerFilterRule(
        providerName: typeof(ApiLoggerProvider).FullName,
        categoryName: "*",
        logLevel: LogLevel.Error,
        filter: null));
});

#endregion

#endregion

var app = builder.Build();

#region Localization

// Set the default culture globally for the application's threads. This ensures that, unless overridden, all culture-dependent
// operations (e.g., number and date formatting) and resource lookups default to the specified language.
var cultureInfo = new CultureInfo("en");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo; // Sets the default culture for data operations (e.g., DateTime, decimal).
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo; // Sets the default culture for resource file lookups (e.g., .resx files).

var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;

// This enables dynamic culture determination for each request based on the following priority order:
// 1. QueryStringRequestCultureProvider: If a query string like ?culture=es&ui-culture=es is present, it takes the highest precedence.
// 2. CookieRequestCultureProvider: If a culture is stored in a cookie, it will be used next.
// 3. AcceptLanguageHeaderRequestCultureProvider: The browser's Accept-Language header determines the culture if no query string or cookie is present.
// 4. DefaultRequestCulture: If none of the above providers set a culture, the default culture (configured in RequestLocalizationOptions) is used.
// NOTE: Thread-level defaults (CultureInfo.DefaultThreadCurrentCulture and CultureInfo.DefaultThreadCurrentUICulture) are overridden per request
// if a culture is determined by any of the RequestLocalization providers.
app.UseRequestLocalization(localizationOptions);

#endregion

#region Cors

// Apply CORS policy only for browser-based requests to ensure proper restrictions and avoid returning CORS headers for backend-to-backend calls.
app.UseWhen(
    context => context.Request.Headers.ContainsKey("Origin"),
    appBuilder => appBuilder.UseCors("ClientApp")
);

#endregion

#region Global Exception Handling

// Fallback handler for unobserved exceptions in Tasks outside the HTTP request pipeline, such as those in background services or
// fire-and-forget operations. This handler is invoked for exceptions that are not awaited or handled explicitly in Task-based operations.
// Common scenarios include unhandled exceptions in EmailBackgroundService or LogBackgroundService.
// Note: This does not prevent the exceptions from being thrown but ensures they are logged and do not crash the process.
TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    e.SetObserved(); // Marks the exception as observed to prevent the process from being terminated.
    using var scope = app.Services.CreateScope();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogCritical(e.Exception, "Unobserved task exception caught globally.");
};

// Fallback handler for unhandled exceptions in non-Task-based operations outside the HTTP request pipeline.
// This includes exceptions in long-running background threads (e.g., Thread, ThreadPool), unmanaged code, or application-level failures.
// Note: This is a last-resort handler for unexpected, unhandled exceptions. While the application may attempt to log these exceptions,
// the process might still terminate depending on the severity of the error.
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    var exception = e.ExceptionObject as Exception;
    if (exception != null)
    {
        using var scope = app.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogCritical(exception, "Unhandled exception caught globally in the AppDomain.");
    }
};

#endregion

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Middleware to inject a JWT token into requests for testing endpoints requiring authorization (for development only)
    app.Use(async (context, next) =>
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            using var scope = app.Services.CreateScope();

            var jwtTokenProvider = app.Services.GetRequiredService<IJwtTokenProvider>();

            var defaultUserName = "conraarq";
            var defaultUserId = "b757fbe8-5724-4d0d-9be5-3826b42e0452";
            var defaultEmail = "conra.arq@gmail.com";

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.NameIdentifier, defaultUserId),
            new Claim(ClaimTypes.Email, defaultEmail),
            new Claim(ClaimTypes.Name, defaultUserName)
        };

            var token = jwtTokenProvider.GenerateAuthenticationToken(TimeSpan.FromMinutes(1), claims.ToArray());
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        await next.Invoke();
    });
}

app.UseRouting();

app.UseMiddleware<AuthErrorMiddleware>();

app.UseAuthentication();

app.UseMiddleware<UserContextMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
