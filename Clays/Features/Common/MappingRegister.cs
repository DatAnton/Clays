using Clays.Domain.Entities;
using Mapster;
using ActionEntity = Clays.Domain.Entities.Action;
using ActionCreate = Clays.Features.Actions.CreateAction;
using ActionUpdateStatus = Clays.Features.Actions.UpdateActionStatus;
using CompanyDetails = Clays.Features.Companies.GetCompany;
using MachineListItem = Clays.Features.Machines.ListMachines.MachineItem;

namespace Clays.Features.Common;

public sealed class MappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Machine, MachineListItem>()
            .Map(destination => destination.ManufacturerName, source => source.Manufacturer.Name)
            .Map(destination => destination.CompanyName, source => source.Company == null ? null : source.Company.Name)
            .Map(destination => destination.CompanyCity, source => source.Company == null ? null : source.Company.City);

        config.NewConfig<Machine, CompanyDetails.MachineItem>()
            .Map(destination => destination.ManufacturerName, source => source.Manufacturer.Name);

        config.NewConfig<User, CompanyDetails.UserSummary>();
        config.NewConfig<Contact, CompanyDetails.ContactItem>();
        config.NewConfig<CompanyComment, CompanyDetails.CommentItem>();
        config.NewConfig<OwnerHistory, CompanyDetails.OwnerHistoryItem>();
        config.NewConfig<ActionEntity, CompanyDetails.ActionItem>()
            .Map(destination => destination.CreatedDate,
                source => new DateTimeOffset(DateTime.SpecifyKind(source.CreatedDate, DateTimeKind.Utc)));
        config.NewConfig<ActionEntity, ActionCreate.Response>()
            .Map(destination => destination.CreatedDate,
                source => new DateTimeOffset(DateTime.SpecifyKind(source.CreatedDate, DateTimeKind.Utc)));
        config.NewConfig<ActionEntity, ActionUpdateStatus.Response>()
            .Map(destination => destination.CreatedDate,
                source => new DateTimeOffset(DateTime.SpecifyKind(source.CreatedDate, DateTimeKind.Utc)));
        config.NewConfig<Company, CompanyDetails.Response>();
    }
}
