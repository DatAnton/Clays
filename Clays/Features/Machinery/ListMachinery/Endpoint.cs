using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using MachineryEntity = Clays.Domain.Entities.Machinery;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Machinery.ListMachinery;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    private const int PageSize = 50;

    public override void Configure() => Get("/api/machinery");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        IQueryable<MachineryEntity> query = context.Machinery.AsNoTracking()
            .Include(machinery => machinery.MachineryLibrary).ThenInclude(entry => entry.Manufacturer)
            .Include(machinery => machinery.Company);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(machinery => EF.Functions.ILike(machinery.MachineryLibrary.Model, pattern)
                || (machinery.VinNumber != null && EF.Functions.ILike(machinery.VinNumber, pattern))
                || (machinery.RegistrationNumber != null && EF.Functions.ILike(machinery.RegistrationNumber, pattern))
                || (machinery.Company != null && EF.Functions.ILike(machinery.Company.Name, pattern))
                || EF.Functions.ILike(machinery.MachineryLibrary.Manufacturer.Name, pattern));
        }
        if (request.ManufacturerId.HasValue)
            query = query.Where(machinery => machinery.MachineryLibrary.ManufacturerId == request.ManufacturerId.Value);
        if (request.MachineryLibraryId.HasValue)
            query = query.Where(machinery => machinery.MachineryLibraryId == request.MachineryLibraryId.Value);
        if (request.Status.HasValue)
            query = query.Where(machinery => machinery.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (int)Math.Min(((long)page - 1) * PageSize, int.MaxValue);
        var machinery = await query.OrderBy(machinery => machinery.MachineryLibrary.Manufacturer.Name).ThenBy(machinery => machinery.MachineryLibrary.Model)
            .ThenBy(machinery => machinery.Id).Skip(offset).Take(PageSize).ToListAsync(cancellationToken);
        await Send.OkAsync(new Response(machinery.Adapt<List<MachineryItem>>(), page, PageSize, totalCount), cancellationToken);
    }
}
