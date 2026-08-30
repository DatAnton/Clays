using Clays.Domain.Entities;

namespace Clays.Application.Common.Interfaces
{
    public interface IMachineService
    {
        Task SaveMachine(Machine machine, CancellationToken cancellationToken);
    }
}
