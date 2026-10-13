namespace Clays.Domain.Entities;

public class VinSearchSession
{
    public int Id { get; set; }
    public string VinStart { get; set; } = string.Empty;
    public string VinEnd { get; set; } = string.Empty;
    public bool IsCompleted{get;set;}
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<VinEntry> VinEntries { get; set; } = new List<VinEntry>();
    
}
