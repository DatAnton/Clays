using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
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
            .Select(company => new CompanyResponse(
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
}

/// <summary>A company row returned by the companies API.</summary>
public sealed record CompanyResponse(
    int Id,
    string Name,
    string? BusinessId,
    string? City,
    string? Address,
    string? MainBusinessLine,
    bool IsOurPartner);

/// <summary>A company page and its pagination metadata.</summary>
public sealed record CompanyPageResponse(
    IReadOnlyList<CompanyResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);