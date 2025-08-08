using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Core.Entities;

public class Order
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Cliente { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Produto { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue)]
    public decimal Valor { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = OrderStatus.Pendente;
    
    public DateTime DataCriacao { get; set; }
    
    public Order()
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
    }
}

public static class OrderStatus
{
    public const string Pendente = "Pendente";
    public const string Processando = "Processando";
    public const string Finalizado = "Finalizado";
}