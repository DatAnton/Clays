using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clays.Controllers;

/// <summary>Provides searchable, paginated company data.</summary>
[ApiController]
[Route("api/companies")]
public sealed class CompaniesController(IApplicationDbContext context) : ControllerBase
{
    private const int PageSize = 100;

    /// <summary>Gets a page of companies, optionally filtered by text, city, business line, or partner status.</summary>
    [HttpGet]
    [ProducesResponseType<CompanyPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyPageResponse>> GetCompanies(
        [FromQuery] int page = 1,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] string? businessLine = null,
        [FromQuery] bool? isOurPartner = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);

        IQueryable<Company> query = context.Companies.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(company =>
                EF.Functions.ILike(company.Name, pattern)
                || (company.BusinessId != null && EF.Functions.ILike(company.BusinessId, pattern))
                || (company.City != null && EF.Functions.ILike(company.City, pattern))
                || (company.Address != null && EF.Functions.ILike(company.Address, pattern))
                || (company.MainBusinessLine != null && EF.Functions.ILike(company.MainBusinessLine, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var pattern = $"%{city.Trim()}%";
            query = query.Where(company => company.City != null && EF.Functions.ILike(company.City, pattern));
        }

        if (!string.IsNullOrWhiteSpace(businessLine))
        {
            var pattern = $"%{businessLine.Trim()}%";
            query = query.Where(company => company.MainBusinessLine != null
                && EF.Functions.ILike(company.MainBusinessLine, pattern));
        }

        if (isOurPartner.HasValue)
        {
            query = query.Where(company => company.IsOurPartner == isOurPartner.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (int)Math.Min(((long)page - 1) * PageSize, int.MaxValue);
        var companies = await query
            .OrderBy(company => company.Name)
            .ThenBy(company => company.Id)
            .Skip(offset)
            .Take(PageSize)
            .Select(company => new CompanyListItemResponse(
                company.Id,
                company.Name,
                company.BusinessId,
                company.City,
                company.Address,
                company.MainBusinessLine,
                company.IsOurPartner))
            .ToListAsync(cancellationToken);

        return Ok(new CompanyPageResponse(companies, page, PageSize, totalCount));
    }

    /// <summary>Gets one company together with its related entities.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CompanyDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDetailsResponse>> GetCompanyById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var company = await context.Companies
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.CreatedBy)
            .Include(item => item.Contacts)
            .Include(item => item.Comments)
                .ThenInclude(comment => comment.Author)
            .Include(item => item.Machines)
                .ThenInclude(machine => machine.Manufacturer)
            .Include(item => item.OwnerHistory)
                .ThenInclude(history => history.Machine)
                    .ThenInclude(machine => machine.Manufacturer)
            .Include(item => item.Actions)
                .ThenInclude(action => action.Author)
            .Include(item => item.Actions)
                .ThenInclude(action => action.AssignedUser)
            .Include(item => item.Actions)
                .ThenInclude(action => action.Machine)
                    .ThenInclude(machine => machine!.Manufacturer)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (company is null)
        {
            return NotFound();
        }

        return Ok(new CompanyDetailsResponse(
            company.Id,
            company.Name,
            company.City,
            company.Address,
            company.IsOurPartner,
            company.BusinessId,
            company.MainBusinessLine,
            company.Status,
            company.Note,
            MapUser(company.CreatedBy),
            company.Contacts.Select(contact => new ContactResponse(
                contact.Id,
                contact.FirstName,
                contact.LastName,
                contact.PhoneNumber,
                contact.Email,
                contact.CompanyId)).ToList(),
            company.Comments.Select(comment => new CompanyCommentResponse(
                comment.Id,
                comment.CompanyId,
                comment.Text,
                MapUser(comment.Author)!,
                comment.CreatedAtUtc)).ToList(),
            company.Machines.Select(MapMachine).ToList(),
            company.OwnerHistory.Select(history => new OwnerHistoryResponse(
                history.Id,
                MapMachine(history.Machine))).ToList(),
            company.Actions.Select(action => new ActionResponse(
                action.Id,
                action.AuthorId,
                MapUser(action.Author)!,
                action.AssignedUserId,
                MapUser(action.AssignedUser),
                action.Type,
                action.Status,
                action.Note,
                new DateTimeOffset(DateTime.SpecifyKind(action.CreatedDate, DateTimeKind.Utc)),
                action.ActionWeek,
                action.CompanyId,
                action.MachineId,
                action.Machine is null ? null : MapMachine(action.Machine))).ToList()));
    }

    private static UserSummaryResponse? MapUser(User? user) => user is null
        ? null
        : new UserSummaryResponse(user.Id, user.FirstName, user.LastName, user.UserName);

    private static MachineResponse MapMachine(Machine machine) => new(
        machine.Id,
        machine.ManufacturerId,
        machine.Manufacturer.Name,
        machine.Model,
        machine.Year,
        machine.VinNumber,
        machine.RegistrationNumber,
        machine.CountryOfImport,
        machine.PurchaseYear,
        machine.Status,
        machine.CreatedById);
}

/// <summary>A company row returned by the companies API.</summary>
public sealed record CompanyListItemResponse(
    int Id,
    string Name,
    string? BusinessId,
    string? City,
    string? Address,
    string? MainBusinessLine,
    bool IsOurPartner);

/// <summary>A company page and its pagination metadata.</summary>
public sealed record CompanyPageResponse(
    IReadOnlyList<CompanyListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);