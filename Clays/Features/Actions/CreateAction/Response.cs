using Clays.Features.Companies.GetCompany;
using Clays.Domain.Entities.Enums;

namespace Clays.Features.Actions.CreateAction;

public sealed record Response(
    int Id, int AuthorId, UserSummary Author, int? AssignedUserId, UserSummary? AssignedUser,
    ActionType Type, ActionStatus Status, string? Note, DateTimeOffset CreatedDate,
    int ActionWeek, int CompanyId, int? MachineId, MachineItem? Machine);
