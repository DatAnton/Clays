using Clays.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clays.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Machine> Machinery { get; set; }
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

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
