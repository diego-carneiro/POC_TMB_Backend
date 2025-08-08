using Microsoft.EntityFrameworkCore;
using OrderManagement.Core.Entities;

namespace OrderManagement.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cliente).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Produto).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Valor).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DataCriacao).IsRequired();
            
            entity.HasIndex(e => e.DataCriacao);
            entity.HasIndex(e => e.Status);
        });

        base.OnModelCreating(modelBuilder);
    }
}