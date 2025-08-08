using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderManagement.Core.Interfaces;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Repositories;
using OrderManagement.Infrastructure.Services;
using OrderManagement.Worker;
using OrderManagement.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Serviços básicos do Worker
builder.Services.AddHostedService<Worker>();

// Configuração do banco de dados
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injeção de dependências
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IMessageService, AzureServiceBusService>();

// Serviço de processamento de pedidos
builder.Services.AddHostedService<OrderProcessingService>();

var host = builder.Build();
host.Run();
