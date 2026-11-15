
using Microsoft.EntityFrameworkCore;
using Reunioes.Api.Models;

namespace Reunioes.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tabela de salas
        public DbSet<Sala> Salas { get; set; } = null!;

        // Tabela de reservas
        public DbSet<Reserva> Reservas { get; set; } = null!;
    }
}
