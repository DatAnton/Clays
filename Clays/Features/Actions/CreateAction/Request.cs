using Clays.Domain.Entities.Enums;

namespace Clays.Features.Actions.CreateAction;

public sealed class Request
{
    public int? AssignedUserId { get; init; }
    public ActionType Type { get; init; }
    public ActionStatus Status { get; init; } = ActionStatus.Todo;
    public string? Note { get; init; }
    public int ActionWeek { get; init; }
    public int CompanyId { get; init; }
    public int? MachineId { get; init; }
}
