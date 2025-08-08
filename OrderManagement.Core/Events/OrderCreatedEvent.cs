namespace OrderManagement.Core.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataCriacao { get; set; }
}