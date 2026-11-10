using Clays.Domain.Entities;

namespace Clays.Application.Common.Interfaces
{
    public interface IMachineryService
    {
        Task SaveMachinery(Machinery machinery, CancellationToken cancellationToken);
    }
}
