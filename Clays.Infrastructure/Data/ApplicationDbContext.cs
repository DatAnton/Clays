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
    }
}
