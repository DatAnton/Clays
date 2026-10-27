using Clays.Application.Common.Interfaces;
using Clays.Application.Common.Interfaces.Scrapers;
using Clays.Application.Common.Configuration;
using Clays.Configuration;
using Clays.Extensions;
using Clays.Infrastructure.Data;
using Clays.Infrastructure.Services;
using Clays.Infrastructure.Services.Scrapers;
using Clays.Services;
using Hangfire;
using Hangfire.Common;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Soenneker.Hangfire.BasicAuthentication;
using Soenneker.Hangfire.BasicAuthentication.Registrars;


var builder = WebApplication.CreateBuilder(args);
var auth0Domain = builder.Configuration["Auth0:Domain"];
var auth0Audience = builder.Configuration["Auth0:Audience"];
if (string.IsNullOrWhiteSpace(auth0Domain) || string.IsNullOrWhiteSpace(auth0Audience))
{
    throw new InvalidOperationException("Auth0:Domain and Auth0:Audience must be configured.");
}

var applicationConnectionString = builder.Configuration.GetConnectionString("ApplicationContext")
    ?? throw new InvalidOperationException("Connection string 'ApplicationContext' was not found.");

// Add services to the container.

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = auth0Domain;
        options.Audience = auth0Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "sub"
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers(options => options.Filters.Add(new AuthorizeFilter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
{
    options.UseNpgsql(applicationConnectionString);
});

builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(applicationConnectionString)));

builder.Services.AddHangfireServer();
builder.Services.AddHangfireBasicAuth();

builder.Services.AddOptions<HangfireJobSchedules>()
    .Bind(builder.Configuration.GetRequiredSection(HangfireJobSchedules.SectionName))
    .ValidateOnStart();

builder.Services.AddOptions<EarthConfiguration>()
    .Bind(builder.Configuration.GetRequiredSection(EarthConfiguration.SectionName))
    .ValidateOnStart();

builder.Services.AddOptions<CompanyScraperConfiguration>()
    .Bind(builder.Configuration.GetRequiredSection(CompanyScraperConfiguration.SectionName))
    .Validate(configuration => Uri.TryCreate(configuration.EndpointUrl, UriKind.Absolute, out var endpoint)
        && (endpoint.Scheme == Uri.UriSchemeHttp || endpoint.Scheme == Uri.UriSchemeHttps),
        "CompanyScraperConfiguration:EndpointUrl must be an absolute HTTP or HTTPS URL.")
    .ValidateOnStart();

builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IKiertonetScraper, KiertonetScraper>();
builder.Services.AddHttpClient<ICompanyScraper, CompanyScraper>();
builder.Services.AddHttpClient<IEarthScraper, EarthScraper>();
builder.Services.AddHttpClient<IEarthAuthService, EarthAuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IScopedProcessingService, ScopedProcessingService>();

builder.Services.AddTelegramBotClient(builder.Configuration);
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();

var app = builder.Build();

try
{
    app.MigrateDatabase<ApplicationDbContext>();
    GlobalJobFilters.Filters.Add(new HangfireExceptionLoggingFilter(app.Services));

    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider
            .GetRequiredService<IScopedProcessingService>()
            .AddOrUpdateRecurringJobs();
    }

    // Configure the HTTP request pipeline.
    app.UseMiddleware<ExceptionLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHangfireAuthorized();

    ConfigureHangfire(app, builder.Configuration["Hangfire:Url"] ?? "/hangfire");

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.MapControllers();

    app.MapFallbackToFile("index.html");

    await app.RunAsync();
}
catch (Exception exception)
{
    await ExceptionLogWriter.TryWriteAsync(app.Services, exception, "Application startup or host execution");
    throw;
}

void ConfigureHangfire(IApplicationBuilder app, string dashboardPath)
{
    app.UseHangfireDashboard(dashboardPath, new DashboardOptions
    {
        Authorization = [new HangfireAuthorizationFilter()]
    });
}