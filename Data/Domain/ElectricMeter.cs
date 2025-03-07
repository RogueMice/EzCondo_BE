using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class ElectricMeter
{
    public Guid Id { get; set; }

    public string MeterNumber { get; set; } = null!;

    public DateOnly InstallationDate { get; set; }

    public Guid ApartmentId { get; set; }

    public virtual Apartment Apartment { get; set; } = null!;

    public virtual ICollection<ElectricReading> ElectricReadings { get; set; } = new List<ElectricReading>();
}
