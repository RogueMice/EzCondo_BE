using System;
using System.Collections.Generic;

namespace EzCondo_Data.Temp;

public partial class IncidentDetail
{
    public Guid Id { get; set; }

    public Guid IncidentId { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime UploadAt { get; set; }

    public virtual Incident Incident { get; set; } = null!;
}
