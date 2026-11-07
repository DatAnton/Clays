using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Features.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Clays.Services;

namespace Clays.Features.Users.CreateUser;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/api/users");
        Policies(AdminAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await context.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            await SendDuplicateEmailAsync(cancellationToken);
            return;
        }

        var user = new User
        {
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };
        context.Users.Add(user);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Users_Email" })
        {
            await SendDuplicateEmailAsync(cancellationToken);
            return;
        }

        await EndpointResponse.JsonAsync(HttpContext,
            new Response(user.Id, user.Email, user.FirstName, user.LastName), 201, cancellationToken);
    }

    private Task SendDuplicateEmailAsync(CancellationToken cancellationToken) =>
        EndpointResponse.JsonAsync(HttpContext, new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "A user with this email already exists."
        }, StatusCodes.Status409Conflict, cancellationToken);
}
