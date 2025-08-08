using OrderManagement.Core.Entities;
using OrderManagement.Core.Events;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Worker.Services;

public class OrderProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(IServiceProvider serviceProvider, ILogger<OrderProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();
        
        await messageService.StartProcessingAsync<OrderCreatedEvent>("order-created", ProcessOrderAsync);
        
        // Serviço segue rodando
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessOrderAsync(OrderCreatedEvent orderEvent)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            
            // Buscar o pedido
            var order = await orderRepository.GetByIdAsync(orderEvent.OrderId);
            if (order == null) return;

            // Atualizar para "Processando"
            order.Status = OrderStatus.Processando;
            await orderRepository.UpdateAsync(order);
            _logger.LogInformation("Order {OrderId} status updated to Processando", order.Id);

            // Aguardar 5 segundos
            await Task.Delay(5000);

            // Atualizar para "Finalizado"
            order.Status = OrderStatus.Finalizado;
            await orderRepository.UpdateAsync(order);
            _logger.LogInformation("Order {OrderId} status updated to Finalizado", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", orderEvent.OrderId);
        }
    }
}