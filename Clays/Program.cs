using Clays.Application.Common.Interfaces;
using Clays.Extensions;
using Clays.Infrastructure.Data;
using Clays.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
{
    var connectString = builder.Configuration.GetConnectionString("ApplicationContext");
    
    options.UseSqlite(connectString);
});

builder.Services.AddScoped<IMachineService, MachineService>();

var app = builder.Build();

app.MigrateDatabase<ApplicationDbContext>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
