using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;

namespace Clays.Infrastructure.Services
{
    public class MachineService(IApplicationDbContext context) : IMachineService
    {
        public async Task SaveMachine(Machine machine, CancellationToken cancellationToken)
        {
            context.Machines.Add(machine);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
