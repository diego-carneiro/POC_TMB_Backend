using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.DTOs;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Events;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMessageService _messageService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderRepository orderRepository, 
        IMessageService messageService,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _messageService = messageService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
    {
        try
        {
            var order = new Order
            {
                Cliente = createOrderDto.Cliente,
                Produto = createOrderDto.Produto,
                Valor = createOrderDto.Valor,
                Status = OrderStatus.Pendente
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Enviar evento para Service Bus
            var orderEvent = new OrderCreatedEvent
            {
                OrderId = createdOrder.Id,
                Cliente = createdOrder.Cliente,
                Produto = createdOrder.Produto,
                Valor = createdOrder.Valor,
                DataCriacao = createdOrder.DataCriacao
            };

            await _messageService.SendMessageAsync(orderEvent, "order-created");

            var orderDto = new OrderDto
            {
                Id = createdOrder.Id,
                Cliente = createdOrder.Cliente,
                Produto = createdOrder.Produto,
                Valor = createdOrder.Valor,
                Status = createdOrder.Status,
                DataCriacao = createdOrder.DataCriacao
            };

            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync();
            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                Cliente = o.Cliente,
                Produto = o.Produto,
                Valor = o.Valor,
                Status = o.Status,
                DataCriacao = o.DataCriacao
            });

            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                Cliente = order.Cliente,
                Produto = order.Produto,
                Valor = order.Valor,
                Status = order.Status,
                DataCriacao = order.DataCriacao
            };

            return Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order {OrderId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
    try
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound($"Pedido com ID {id} não encontrado");
        }

        await _orderRepository.DeleteAsync(id);
        
        _logger.LogInformation("Order {OrderId} deleted successfully", id);
        
        return NoContent(); 
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting order {OrderId}", id);
        return StatusCode(500, "Erro interno do servidor");
    }
}
}