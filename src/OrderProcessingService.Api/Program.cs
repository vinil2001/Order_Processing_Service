using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Core.DTOs;
using OrderProcessingService.Core.Interfaces;
using OrderProcessingService.Core.Services;
using OrderProcessingService.Infrastructure.Data;
using OrderProcessingService.Infrastructure.Queue;
using OrderProcessingService.Infrastructure.Repositories;
using OrderProcessingService.Infrastructure.Services;
using StackExchange.Redis;
using OrderProcessingSvc = OrderProcessingService.Core.Services.OrderProcessingService;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

// DI
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IOrderQueue, RedisOrderQueue>();
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();
builder.Services.AddScoped<OrderProcessingSvc>();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal API: POST /orders
app.MapPost("/orders", async (OrderProcessingSvc service, CreateOrderRequest req, CancellationToken ct) =>
{
    var result = await service.SubmitAsync(req, ct);
    return Results.Accepted($"/orders/{result.OrderId}", result);
});

app.Run();