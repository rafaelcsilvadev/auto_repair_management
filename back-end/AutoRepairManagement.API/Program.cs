
using AutoRepairManagement.API.Core.Data;
using AutoRepairManagement.API.Features.Client.EndPoints;
using AutoRepairManagement.API.Features.Client.Services;
using AutoRepairManagement.API.Features.ServiceOrder.EndPoints;
using AutoRepairManagement.API.Features.ServiceOrder.Services;
using AutoRepairManagement.API.Features.Vehicle.EndPoints;
using AutoRepairManagement.API.Features.Vehicle.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapClientEndPoints();
app.MapVehicleEndPoints();
app.MapServiceOrderEndPoints();

app.Run();