using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Core.Interfaces;
using OrderProcessingService.Infrastructure.Data;
using OrderProcessingService.Infrastructure.Queue;
using OrderProcessingService.Infrastructure.Repositories;
using OrderProcessingService.Infrastructure.Services;
using OrderProcessingService.Worker.Workers;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

// DI
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IOrderQueue, RedisOrderQueue>();
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();

builder.Services.AddHostedService<OrderProcessingWorker>();

var host = builder.Build();
host.Run();