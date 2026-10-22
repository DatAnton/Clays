using System.ComponentModel.DataAnnotations;
using Clays.Domain.Entities.Enums;

namespace Clays.Models;

/// <summary>Represents a company, including its contacts, comments, ownership history, and actions.</summary>
public sealed record CompanyDetailsResponse(
    int Id,
    string Name,
    string? City,
    string? Address,
    bool IsOurPartner,
    string? BusinessId,
    string? MainBusinessLine,
    CompanyStatus? Status,
    string? Note,
    UserSummaryResponse? CreatedBy,
    IReadOnlyList<ContactResponse> Contacts,
    IReadOnlyList<CompanyCommentResponse> Comments,
    IReadOnlyList<MachineResponse> Machines,
    IReadOnlyList<OwnerHistoryResponse> OwnerHistory,
    IReadOnlyList<ActionResponse> Actions);

/// <summary>Represents a user without authentication credentials.</summary>
public sealed record UserSummaryResponse(int Id, string? FirstName, string? LastName, string? UserName);

/// <summary>Represents a machine and its manufacturer.</summary>
public sealed record MachineResponse(
    int Id,
    int ManufacturerId,
    string ManufacturerName,
    string Model,
    int? Year,
    string? VinNumber,
    string? RegistrationNumber,
    string? CountryOfImport,
    int? PurchaseYear,
    MachineStatus? Status,
    int CreatedById);

/// <summary>Represents a company contact.</summary>
public sealed record ContactResponse(
    int Id,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Email,
    int CompanyId);

/// <summary>Represents a company comment and its author.</summary>
public sealed record CompanyCommentResponse(
    int Id,
    int CompanyId,
    string Text,
    UserSummaryResponse Author,
    DateTimeOffset CreatedAtUtc);

/// <summary>Represents a machine previously owned by a company.</summary>
public sealed record OwnerHistoryResponse(int Id, MachineResponse Machine);

/// <summary>Represents a company action and related users and machine.</summary>
public sealed record ActionResponse(
    int Id,
    int AuthorId,
    UserSummaryResponse Author,
    int? AssignedUserId,
    UserSummaryResponse? AssignedUser,
    ActionType Type,
    ActionStatus Status,
    string? Note,
    DateTimeOffset CreatedDate,
    int ActionWeek,
    int CompanyId,
    int? MachineId,
    MachineResponse? Machine);

/// <summary>Payload for creating a company comment.</summary>
public sealed record CreateCompanyCommentRequest
{
    [Required, MaxLength(4000)]
    public required string Text { get; init; }
}

/// <summary>Payload for adding a machine.</summary>
public sealed record CreateMachineRequest
{
    [Range(1, int.MaxValue)]
    public required int ManufacturerId { get; init; }

    [Required, MaxLength(200)]
    public required string Model { get; init; }

    [Range(1800, 2200)]
    public int? Year { get; init; }

    [MaxLength(100)]
    public string? VinNumber { get; init; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; init; }

    [MaxLength(100)]
    public string? CountryOfImport { get; init; }

    [Range(1800, 2200)]
    public int? PurchaseYear { get; init; }

    public MachineStatus? Status { get; init; }

    [Range(1, int.MaxValue)]
    public int? CompanyId { get; init; }
}

/// <summary>Payload for creating an action.</summary>
public sealed record CreateActionRequest
{
    [Range(1, int.MaxValue)]
    public int? AssignedUserId { get; init; }

    public required ActionType Type { get; init; }
    public ActionStatus Status { get; init; } = ActionStatus.Todo;

    [MaxLength(4000)]
    public string? Note { get; init; }

    [Range(1, 53)]
    public required int ActionWeek { get; init; }

    [Range(1, int.MaxValue)]
    public required int CompanyId { get; init; }

    [Range(1, int.MaxValue)]
    public int? MachineId { get; init; }
}

/// <summary>Payload for changing an action's status.</summary>
public sealed record UpdateActionStatusRequest
{
    public required ActionStatus Status { get; init; }
}

/// <summary>Payload for adding a contact to a company.</summary>
public sealed record CreateContactRequest
{
    [MaxLength(200)]
    public string? FirstName { get; init; }

    [MaxLength(200)]
    public string? LastName { get; init; }

    [MaxLength(50)]
    public string? PhoneNumber { get; init; }

    [EmailAddress, MaxLength(320)]
    public string? Email { get; init; }

    [Range(1, int.MaxValue)]
    public required int CompanyId { get; init; }
}
