using Clays.Application.Common.Interfaces;
using Clays.Application.Common.Interfaces.Scrapers;
using Clays.Configuration;
using Clays.Extensions;
using Clays.Infrastructure.Data;
using Clays.Infrastructure.Services;
using Clays.Infrastructure.Services.Scrapers;
using Clays.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Soenneker.Hangfire.BasicAuthentication;
using Soenneker.Hangfire.BasicAuthentication.Registrars;


var builder = WebApplication.CreateBuilder(args);
var applicationConnectionString = builder.Configuration.GetConnectionString("ApplicationContext")
    ?? throw new InvalidOperationException("Connection string 'ApplicationContext' was not found.");

// Add services to the container.

builder.Services.AddControllers();
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

builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IKiertonetScraper, KiertonetScraper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IScopedProcessingService, ScopedProcessingService>();

builder.Services.AddTelegramBotClient(builder.Configuration);
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();

var app = builder.Build();

app.MigrateDatabase<ApplicationDbContext>();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider
        .GetRequiredService<IScopedProcessingService>()
        .AddOrUpdateRecurringJobs();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseHangfireAuthorized();

app.UseAuthorization();

ConfigureHangfire(app, builder.Configuration["Hangfire:Url"] ?? "/hangfire");

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

void ConfigureHangfire(IApplicationBuilder app, string dashboardPath)
{
    app.UseHangfireDashboard(dashboardPath, new DashboardOptions
    {
        Authorization = [new HangfireAuthorizationFilter()]
    });
}