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
        public DbSet<Company> Companies { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorLog>().ToTable("Logs");
            modelBuilder.Entity<Company>()
                .Property(company => company.IsOurPartner)
                .HasDefaultValue(false);

            modelBuilder.Entity<Company>()
                .HasMany(company => company.Contacts)
                .WithOne(contact => contact.Company)
                .HasForeignKey(contact => contact.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
