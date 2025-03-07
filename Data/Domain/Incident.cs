using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class Incident
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime ReportedAt { get; set; }

    public string Status { get; set; } = null!;

    public int Priority { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<IncidentDetail> IncidentDetails { get; set; } = new List<IncidentDetail>();

    public virtual User User { get; set; } = null!;
}
