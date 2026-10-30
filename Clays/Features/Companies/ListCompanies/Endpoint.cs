using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using FastEndpoints;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Companies.ListCompanies;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    private const int PageSize = 100;

    public override void Configure() => Get("/api/companies");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        IQueryable<Company> query = context.Companies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(company => EF.Functions.ILike(company.Name, pattern)
                || (company.BusinessId != null && EF.Functions.ILike(company.BusinessId, pattern))
                || (company.City != null && EF.Functions.ILike(company.City, pattern))
                || (company.Address != null && EF.Functions.ILike(company.Address, pattern))
                || (company.MainBusinessLine != null && EF.Functions.ILike(company.MainBusinessLine, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var pattern = $"%{request.City.Trim()}%";
            query = query.Where(company => company.City != null && EF.Functions.ILike(company.City, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.BusinessLine))
        {
            var pattern = $"%{request.BusinessLine.Trim()}%";
            query = query.Where(company => company.MainBusinessLine != null
                && EF.Functions.ILike(company.MainBusinessLine, pattern));
        }

        if (request.IsOurPartner.HasValue)
            query = query.Where(company => company.IsOurPartner == request.IsOurPartner.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (int)Math.Min(((long)page - 1) * PageSize, int.MaxValue);
        var items = await query.OrderBy(company => company.Name).ThenBy(company => company.Id)
            .Skip(offset).Take(PageSize).ToListAsync(cancellationToken);

        await Send.OkAsync(new Response(items.Adapt<List<CompanyItem>>(), page, PageSize, totalCount), cancellationToken);
    }
}
