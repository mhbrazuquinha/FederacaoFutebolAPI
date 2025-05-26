using Microsoft.EntityFrameworkCore;
using FederacaoFutebolApi.Models;

namespace FederacaoFutebolApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Time> Times { get; set; }
        public DbSet<Jogador> Jogadores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Time>()
                .HasMany(t => t.Jogadores)
                .WithOne(j => j.Time)
                .HasForeignKey(j => j.TimeId)
                .OnDelete(DeleteBehavior.Cascade); // Deleta o time e os jogadores
        }
    }
}