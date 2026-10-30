using Clays.Domain.Entities.Enums;
using Clays.Features.Companies.GetCompany;

namespace Clays.Features.Actions.UpdateActionStatus;

public sealed record Response(
	int Id, int AuthorId, UserSummary Author, int? AssignedUserId, UserSummary? AssignedUser,
	ActionType Type, ActionStatus Status, string? Note, DateTimeOffset CreatedDate,
	int ActionWeek, int CompanyId, int? MachineId, MachineItem? Machine);
