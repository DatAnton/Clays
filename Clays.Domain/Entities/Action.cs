using Clays.Domain.Entities.Enums;

namespace Clays.Domain.Entities;

public class Action
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public int? AssignedUserId { get; set; }
    public ActionType Type { get; set; }
    public ActionStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ActionWeek { get; set; }
    public int CompanyId { get; set; }
    public int? MachineId { get; set; }

    public User Author { get; set; } = null!;
    public User? AssignedUser { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public Machine? Machine { get; set; }
}
