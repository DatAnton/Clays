using System;

namespace Clays.Domain.Entities;

public class VinEntry
{
    public int Id {get;set;}
    public string VinCode { get; set; }
    public bool IsFound { get; set; }
    public int VinSearchSessionId { get; set; }
    public VinSearchSession VinSearchSession { get; set; }
    
}
