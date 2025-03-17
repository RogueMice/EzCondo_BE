using System;
using System.Collections.Generic;

namespace EzCondo_Data.Context;

public partial class WaterMeter
{
    public Guid Id { get; set; }

    public string MeterNumber { get; set; } = null!;

    public DateOnly InstallationDate { get; set; }

    public Guid ApartmentId { get; set; }

    public virtual Apartment Apartment { get; set; } = null!;

    public virtual ICollection<WaterReading> WaterReadings { get; set; } = new List<WaterReading>();
}
