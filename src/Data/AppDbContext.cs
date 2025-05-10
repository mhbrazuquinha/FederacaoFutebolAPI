// Data/AppDbContext.cs
using FedercaoFutebolAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FedercaoFutebolAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Time> Times => Set<Time>();
    public DbSet<Jogador> Jogadores => Set<Jogador>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração do relacionamento
        modelBuilder.Entity<Jogador>()
            .HasOne(j => j.Time)
            .WithMany(t => t.Jogadores)
            .HasForeignKey(j => j.TimeId);
    }
}