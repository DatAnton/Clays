using Clays.Domain.Entities.Enums;

namespace Clays.Features.Actions.UpdateActionStatus;

public sealed record Request(ActionStatus Status)
{
    public int Id { get; set; }
}
