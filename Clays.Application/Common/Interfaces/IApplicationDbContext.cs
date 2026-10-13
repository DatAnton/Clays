using Clays.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clays.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Machine> Machines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
