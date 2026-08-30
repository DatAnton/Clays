using Clays.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clays.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Machine> Machines { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
