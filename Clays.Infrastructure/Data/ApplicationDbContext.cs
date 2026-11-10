using Clays.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Clays.Domain.Entities;
using Clays.Domain.Constants;

namespace Clays.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<Machinery> Machinery { get; set; }
        public DbSet<MachineryLibrary> MachineryLibrary { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
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
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(role => role.Name).HasMaxLength(50);
                entity.HasIndex(role => role.Name).IsUnique();
                entity.HasData(
                    new Role { Id = 1, Name = RoleNames.Admin },
                    new Role { Id = 2, Name = RoleNames.Staff },
                    new Role { Id = 3, Name = RoleNames.ProjectWorker });
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
                entity.HasOne(userRole => userRole.User).WithMany(user => user.UserRoles)
                    .HasForeignKey(userRole => userRole.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(userRole => userRole.Role).WithMany()
                    .HasForeignKey(userRole => userRole.RoleId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(user => user.Auth0Subject).IsUnique();
                entity.Property(user => user.Email).HasMaxLength(320);
                entity.HasIndex(user => user.Email).IsUnique();
                entity.ToTable(table => table.HasCheckConstraint("CK_Users_Email_Normalized",
                    "\"Email\" = lower(btrim(\"Email\"))"));
            });

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

            modelBuilder.Entity<MachineryLibrary>()
                .HasIndex(entry => new { entry.ManufacturerId, entry.Model })
                .IsUnique();

            modelBuilder.Entity<MachineryLibrary>()
                .HasOne(entry => entry.Manufacturer)
                .WithMany()
                .HasForeignKey(entry => entry.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Machinery>()
                .HasOne(machinery => machinery.MachineryLibrary)
                .WithMany()
                .HasForeignKey(machinery => machinery.MachineryLibraryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Machinery>()
                .HasOne(machinery => machinery.CreatedBy)
                .WithMany()
                .HasForeignKey(machinery => machinery.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Machinery>()
                .HasOne(machinery => machinery.Company)
                .WithMany(company => company.Machinery)
                .HasForeignKey(machinery => machinery.CompanyId)
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
                .HasOne(history => history.Machinery)
                .WithMany()
                .HasForeignKey(history => history.MachineryId)
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
                .HasOne(action => action.Machinery)
                .WithMany()
                .HasForeignKey(action => action.MachineryId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
