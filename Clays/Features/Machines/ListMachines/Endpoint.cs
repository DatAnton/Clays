using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Machines.ListMachines;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    private const int PageSize = 50;

    public override void Configure() => Get("/api/machines");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        IQueryable<Machine> query = context.Machines.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(machine => EF.Functions.ILike(machine.Model, pattern)
                || (machine.VinNumber != null && EF.Functions.ILike(machine.VinNumber, pattern))
                || (machine.RegistrationNumber != null && EF.Functions.ILike(machine.RegistrationNumber, pattern))
                || (machine.Company != null && EF.Functions.ILike(machine.Company.Name, pattern))
                || EF.Functions.ILike(machine.Manufacturer.Name, pattern));
        }
        if (request.ManufacturerId.HasValue)
            query = query.Where(machine => machine.ManufacturerId == request.ManufacturerId.Value);
        if (request.Status.HasValue)
            query = query.Where(machine => machine.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var pattern = $"%{request.City.Trim()}%";
            query = query.Where(machine => machine.Company != null && machine.Company.City != null
                && EF.Functions.ILike(machine.Company.City, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (int)Math.Min(((long)page - 1) * PageSize, int.MaxValue);
        var machines = await query.OrderBy(machine => machine.Manufacturer.Name).ThenBy(machine => machine.Model)
            .ThenBy(machine => machine.Id).Skip(offset).Take(PageSize).ToListAsync(cancellationToken);
        await Send.OkAsync(new Response(machines.Adapt<List<MachineItem>>(), page, PageSize, totalCount), cancellationToken);
    }
}
