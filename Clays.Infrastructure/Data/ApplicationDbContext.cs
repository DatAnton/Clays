using Clays.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Clays.Domain.Entities;

namespace Clays.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<Machine> Machines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<VinSearchSession> VinSearchSessions { get; set; }
        public DbSet<VinEntry> VinEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorLog>().ToTable("Logs");
            base.OnModelCreating(modelBuilder);
        }
    }
}
