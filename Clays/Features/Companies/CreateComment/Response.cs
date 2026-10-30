using Clays.Features.Companies.GetCompany;

namespace Clays.Features.Companies.CreateComment;

public sealed record Response(int Id, int CompanyId, string Text, UserSummary Author, DateTimeOffset CreatedAtUtc);
