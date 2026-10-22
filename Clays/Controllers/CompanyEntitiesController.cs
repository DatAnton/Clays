using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Domain.Entities.Enums;
using Clays.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clays.Controllers;

/// <summary>Creates comments, contacts, machines, and company actions.</summary>
[ApiController]
[Route("api")]
public sealed class CompanyEntitiesController(IApplicationDbContext context) : ControllerBase
{
    /// <summary>Adds a comment to a company.</summary>
    [HttpPost("companies/{companyId:int}/comments")]
    [ProducesResponseType<CompanyCommentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyCommentResponse>> CreateCompanyComment(
        int companyId,
        CreateCompanyCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Companies.AnyAsync(company => company.Id == companyId, cancellationToken))
        {
            return NotFound($"Company {companyId} was not found.");
        }

        var authorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var author = await context.Users.AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == authorId, cancellationToken);
        if (author is null)
        {
            return Unauthorized();
        }

        var comment = new CompanyComment
        {
            CompanyId = companyId,
            Text = request.Text.Trim(),
            AuthorId = author.Id,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        context.CompanyComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new CompanyCommentResponse(
            comment.Id,
            comment.CompanyId,
            comment.Text,
            ToUserResponse(author),
            comment.CreatedAtUtc));
    }

    /// <summary>Adds a machine.</summary>
    [HttpPost("machines")]
    [ProducesResponseType<MachineResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachineResponse>> CreateMachine(
        CreateMachineRequest request,
        CancellationToken cancellationToken = default)
    {
        var manufacturer = await context.Manufacturers.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.ManufacturerId, cancellationToken);
        if (manufacturer is null)
        {
            return NotFound($"Manufacturer {request.ManufacturerId} was not found.");
        }

        if (request.CompanyId.HasValue
            && !await context.Companies.AnyAsync(company => company.Id == request.CompanyId.Value, cancellationToken))
        {
            return NotFound($"Company {request.CompanyId} was not found.");
        }

        if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
        {
            return BadRequest("Machine status is invalid.");
        }

        var machine = new Machine
        {
            ManufacturerId = request.ManufacturerId,
            Model = request.Model.Trim(),
            Year = request.Year,
            VinNumber = request.VinNumber,
            RegistrationNumber = request.RegistrationNumber,
            CountryOfImport = request.CountryOfImport,
            PurchaseYear = request.PurchaseYear,
            Status = request.Status,
            CreatedById = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            CompanyId = request.CompanyId
        };

        context.Machines.Add(machine);
        await context.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ToMachineResponse(machine, manufacturer.Name));
    }

    /// <summary>Adds an action to a company.</summary>
    [HttpPost("actions")]
    [ProducesResponseType<ActionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActionResponse>> CreateAction(
        CreateActionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.Type) || !Enum.IsDefined(request.Status))
        {
            return BadRequest("Action type or status is invalid.");
        }

        var companyExists = await context.Companies
            .AnyAsync(company => company.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
        {
            return NotFound($"Company {request.CompanyId} was not found.");
        }

        var authorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var author = await context.Users.AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == authorId, cancellationToken);
        if (author is null)
        {
            return Unauthorized();
        }

        User? assignedUser = null;
        if (request.AssignedUserId.HasValue)
        {
            assignedUser = await context.Users.AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == request.AssignedUserId.Value, cancellationToken);
            if (assignedUser is null)
            {
                return NotFound($"Assigned user {request.AssignedUserId} was not found.");
            }
        }

        if (request.MachineId.HasValue
            && !await context.Machines.AnyAsync(machine => machine.Id == request.MachineId.Value, cancellationToken))
        {
            return NotFound($"Machine {request.MachineId} was not found.");
        }

        var action = new Clays.Domain.Entities.Action
        {
            AuthorId = author.Id,
            AssignedUserId = assignedUser?.Id,
            Type = request.Type,
            Status = request.Status,
            Note = request.Note,
            CreatedDate = DateTime.UtcNow,
            ActionWeek = request.ActionWeek,
            CompanyId = request.CompanyId,
            MachineId = request.MachineId
        };

        context.Actions.Add(action);
        await context.SaveChangesAsync(cancellationToken);

        var savedAction = await LoadActionAsync(action.Id, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ToActionResponse(savedAction!));
    }

    /// <summary>Changes the status of an action.</summary>
    [HttpPatch("actions/{id:int}/status")]
    [ProducesResponseType<ActionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActionResponse>> UpdateActionStatus(
        int id,
        UpdateActionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.Status))
        {
            return BadRequest("Action status is invalid.");
        }

        var action = await context.Actions.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (action is null)
        {
            return NotFound($"Action {id} was not found.");
        }

        action.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        var updatedAction = await LoadActionAsync(action.Id, cancellationToken);
        return Ok(ToActionResponse(updatedAction!));
    }

    /// <summary>Adds a contact to a company.</summary>
    [HttpPost("contacts")]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> CreateContact(
        CreateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Companies.AnyAsync(company => company.Id == request.CompanyId, cancellationToken))
        {
            return NotFound($"Company {request.CompanyId} was not found.");
        }

        var contact = new Contact
        {
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email = request.Email?.Trim(),
            CompanyId = request.CompanyId
        };

        context.Contacts.Add(contact);
        await context.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ToContactResponse(contact));
    }

    private Task<Clays.Domain.Entities.Action?> LoadActionAsync(int id, CancellationToken cancellationToken) =>
        context.Actions
            .AsNoTracking()
            .Include(action => action.Author)
            .Include(action => action.AssignedUser)
            .Include(action => action.Machine)
                .ThenInclude(machine => machine!.Manufacturer)
            .SingleOrDefaultAsync(action => action.Id == id, cancellationToken);

    private static UserSummaryResponse ToUserResponse(User user) =>
        new(user.Id, user.FirstName, user.LastName, user.UserName);

    private static MachineResponse ToMachineResponse(Machine machine, string? manufacturerName = null) => new(
        machine.Id,
        machine.ManufacturerId,
        manufacturerName ?? machine.Manufacturer.Name,
        machine.Model,
        machine.Year,
        machine.VinNumber,
        machine.RegistrationNumber,
        machine.CountryOfImport,
        machine.PurchaseYear,
        machine.Status,
        machine.CreatedById);

    private static ContactResponse ToContactResponse(Contact contact) => new(
        contact.Id,
        contact.FirstName,
        contact.LastName,
        contact.PhoneNumber,
        contact.Email,
        contact.CompanyId);

    private static ActionResponse ToActionResponse(Clays.Domain.Entities.Action action) => new(
        action.Id,
        action.AuthorId,
        ToUserResponse(action.Author),
        action.AssignedUserId,
        action.AssignedUser is null ? null : ToUserResponse(action.AssignedUser),
        action.Type,
        action.Status,
        action.Note,
        new DateTimeOffset(DateTime.SpecifyKind(action.CreatedDate, DateTimeKind.Utc)),
        action.ActionWeek,
        action.CompanyId,
        action.MachineId,
        action.Machine is null ? null : ToMachineResponse(action.Machine));
}
