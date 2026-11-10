using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;

namespace Clays.Infrastructure.Services
{
    public class MachineryService(IApplicationDbContext context) : IMachineryService
    {
        public async Task SaveMachinery(Machinery machinery, CancellationToken cancellationToken)
        {
            context.Machinery.Add(machinery);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
