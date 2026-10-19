using Clays.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clays.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Machine> Machines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<VinSearchSession> VinSearchSessions { get; set; }
        public DbSet<VinEntry> VinEntries { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
