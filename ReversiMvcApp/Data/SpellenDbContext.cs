using Microsoft.EntityFrameworkCore;
using ReversiMvcApp.Models;
using System.Text.Json;

namespace ReversiMvcApp.Data
{
    public class SpellenDbContext : DbContext
    {
        public SpellenDbContext(DbContextOptions<SpellenDbContext> options) : base(options) { }

        public DbSet<Spel> Spellen { get; set; }

    }
}