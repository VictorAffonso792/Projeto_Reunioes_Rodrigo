
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

        public DbSet<Sala> Salas { get; set; } = null!;
    }
}
