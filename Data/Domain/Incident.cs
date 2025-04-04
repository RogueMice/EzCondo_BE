using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class Incident
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime ReportedAt { get; set; }

    public string Status { get; set; } = null!;

    public int Priority { get; set; }

    public virtual ICollection<IncidentImage> IncidentImages { get; set; } = new List<IncidentImage>();

    public virtual User User { get; set; } = null!;
}
