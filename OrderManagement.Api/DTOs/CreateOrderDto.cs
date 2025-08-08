using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Api.DTOs;

public class CreateOrderDto
{
    [Required]
    [StringLength(100)]
    public string Cliente { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Produto { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Valor { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}