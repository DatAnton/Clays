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
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<CompanyComment> CompanyComments { get; set; }
        public DbSet<OwnerHistory> OwnerHistories { get; set; }
        public DbSet<Clays.Domain.Entities.Action> Actions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorLog>().ToTable("Logs");
            modelBuilder.Entity<Manufacturer>().HasData(
                new Manufacturer { Id = 1, Name = "Volvo" },
                new Manufacturer { Id = 2, Name = "Hyundai" },
                new Manufacturer { Id = 3, Name = "Doosan" },
                new Manufacturer { Id = 4, Name = "Develon" },
                new Manufacturer { Id = 5, Name = "Hitachi" },
                new Manufacturer { Id = 6, Name = "Caterpillar" });

            modelBuilder.Entity<Company>()
                .Property(company => company.IsOurPartner)
                .HasDefaultValue(false);

            modelBuilder.Entity<Company>()
                .HasMany(company => company.Contacts)
                .WithOne(contact => contact.Company)
                .HasForeignKey(contact => contact.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Machine>()
                .HasOne(machine => machine.Manufacturer)
                .WithMany()
                .HasForeignKey(machine => machine.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Machine>()
                .HasOne(machine => machine.CreatedBy)
                .WithMany()
                .HasForeignKey(machine => machine.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Machine>()
                .HasOne(machine => machine.Company)
                .WithMany(company => company.Machines)
                .HasForeignKey(machine => machine.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CompanyComment>()
                .HasOne(comment => comment.Company)
                .WithMany(company => company.Comments)
                .HasForeignKey(comment => comment.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyComment>()
                .HasOne(comment => comment.Author)
                .WithMany()
                .HasForeignKey(comment => comment.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompanyComment>()
                .Property(comment => comment.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<OwnerHistory>()
                .HasOne(history => history.Company)
                .WithMany(company => company.OwnerHistory)
                .HasForeignKey(history => history.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OwnerHistory>()
                .HasOne(history => history.Machine)
                .WithMany()
                .HasForeignKey(history => history.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Clays.Domain.Entities.Action>()
                .HasOne(action => action.Author)
                .WithMany()
                .HasForeignKey(action => action.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Clays.Domain.Entities.Action>()
                .HasOne(action => action.AssignedUser)
                .WithMany()
                .HasForeignKey(action => action.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Clays.Domain.Entities.Action>()
                .HasOne(action => action.Company)
                .WithMany(company => company.Actions)
                .HasForeignKey(action => action.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Clays.Domain.Entities.Action>()
                .HasOne(action => action.Machine)
                .WithMany()
                .HasForeignKey(action => action.MachineId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
