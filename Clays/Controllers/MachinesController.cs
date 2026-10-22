using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clays.Controllers;

/// <summary>Provides searchable, paginated machine data.</summary>
[ApiController]
[Route("api/machines")]
public sealed class MachinesController(IApplicationDbContext context) : ControllerBase
{
    private const int PageSize = 50;

    /// <summary>Gets machines with optional text, manufacturer, status, and company-city filters.</summary>
    [HttpGet]
    [ProducesResponseType<MachinePageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MachinePageResponse>> GetMachines(
        [FromQuery] int page = 1,
        [FromQuery] string? search = null,
        [FromQuery] int? manufacturerId = null,
        [FromQuery] MachineStatus? status = null,
        [FromQuery] string? city = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        IQueryable<Machine> query = context.Machines.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(machine =>
                EF.Functions.ILike(machine.Model, pattern)
                || (machine.VinNumber != null && EF.Functions.ILike(machine.VinNumber, pattern))
                || (machine.RegistrationNumber != null && EF.Functions.ILike(machine.RegistrationNumber, pattern))
                || (machine.Company != null && EF.Functions.ILike(machine.Company.Name, pattern))
                || EF.Functions.ILike(machine.Manufacturer.Name, pattern));
        }

        if (manufacturerId.HasValue)
        {
            query = query.Where(machine => machine.ManufacturerId == manufacturerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(machine => machine.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var pattern = $"%{city.Trim()}%";
            query = query.Where(machine => machine.Company != null
                && machine.Company.City != null
                && EF.Functions.ILike(machine.Company.City, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var machines = await query
            .OrderBy(machine => machine.Manufacturer.Name)
            .ThenBy(machine => machine.Model)
            .ThenBy(machine => machine.Id)
            .Skip((int)Math.Min(((long)page - 1) * PageSize, int.MaxValue))
            .Take(PageSize)
            .Select(machine => new MachineListItemResponse(
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
                machine.CompanyId,
                machine.Company == null ? null : machine.Company.Name,
                machine.Company == null ? null : machine.Company.City))
            .ToListAsync(cancellationToken);

        return Ok(new MachinePageResponse(machines, page, PageSize, totalCount));
    }
}

/// <summary>A machine row returned by the machine directory.</summary>
public sealed record MachineListItemResponse(
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
    int? CompanyId,
    string? CompanyName,
    string? CompanyCity);

/// <summary>A page of machines and pagination metadata.</summary>
public sealed record MachinePageResponse(
    IReadOnlyList<MachineListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);