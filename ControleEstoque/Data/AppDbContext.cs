using ControleEstoque.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Movimentacao>()
            .Property(p => p.Preco)
            .HasPrecision(18, 2);
    }

    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }
}