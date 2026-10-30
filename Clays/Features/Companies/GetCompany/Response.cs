using Clays.Domain.Entities.Enums;

namespace Clays.Features.Companies.GetCompany;

public sealed record Response(
    int Id,
    string Name,
    string? City,
    string? Address,
    bool IsOurPartner,
    string? BusinessId,
    string? MainBusinessLine,
    CompanyStatus? Status,
    string? Note,
    UserSummary? CreatedBy,
    IReadOnlyList<ContactItem> Contacts,
    IReadOnlyList<CommentItem> Comments,
    IReadOnlyList<MachineItem> Machines,
    IReadOnlyList<OwnerHistoryItem> OwnerHistory,
    IReadOnlyList<ActionItem> Actions);

public sealed record UserSummary(int Id, string? FirstName, string? LastName, string? UserName);
public sealed record ContactItem(int Id, string? FirstName, string? LastName, string? PhoneNumber, string? Email, int CompanyId);
public sealed record CommentItem(int Id, int CompanyId, string Text, UserSummary Author, DateTimeOffset CreatedAtUtc);
public sealed record MachineItem(
    int Id, int ManufacturerId, string ManufacturerName, string Model, int? Year, string? VinNumber,
    string? RegistrationNumber, string? CountryOfImport, int? PurchaseYear, MachineStatus? Status, int CreatedById);
public sealed record OwnerHistoryItem(int Id, MachineItem Machine);
public sealed record ActionItem(
    int Id, int AuthorId, UserSummary Author, int? AssignedUserId, UserSummary? AssignedUser,
    ActionType Type, ActionStatus Status, string? Note, DateTimeOffset CreatedDate, int ActionWeek,
    int CompanyId, int? MachineId, MachineItem? Machine);
