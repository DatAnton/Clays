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
    IReadOnlyList<MachineryItem> Machinery,
    IReadOnlyList<OwnerHistoryItem> OwnerHistory,
    IReadOnlyList<ActionItem> Actions);

public sealed record UserSummary(int Id, string? FirstName, string? LastName, string? UserName);
public sealed record ContactItem(
    int Id, string? FirstName, string? LastName, string? PhoneNumber, string? Email,
    int CompanyId, int? MachineryId, string? Comment);
public sealed record CommentItem(int Id, int CompanyId, string Text, UserSummary Author, DateTimeOffset CreatedAtUtc);
public sealed record MachineryItem(
    int Id, int ManufacturerId, string ManufacturerName, string Model, int? Year, string? VinNumber,
    string? RegistrationNumber, string? CountryOfImport, int? PurchaseYear, MachineryStatus? Status, int CreatedById, int MachineryLibraryId);
public sealed record OwnerHistoryItem(int Id, MachineryItem Machinery);
public sealed record ActionItem(
    int Id, int AuthorId, UserSummary Author, int? AssignedUserId, UserSummary? AssignedUser,
    ActionType Type, ActionStatus Status, string? Note, DateTimeOffset CreatedDate, int ActionWeek,
    int CompanyId, int? MachineryId, MachineryItem? Machinery);
