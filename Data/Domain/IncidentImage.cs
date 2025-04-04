using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class IncidentImage
{
    public Guid Id { get; set; }

    public Guid IncidentId { get; set; }

    public string FilePath { get; set; } = null!;

    public virtual Incident Incident { get; set; } = null!;
}
