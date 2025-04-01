using System;
using System.Collections.Generic;
using EzCondo_Data.Domain;

namespace EzCondo_Data.Context;

public partial class ElectricMeter
{
    public Guid Id { get; set; }

    public string MeterNumber { get; set; } = null!;

    public DateOnly InstallationDate { get; set; }

    public Guid ApartmentId { get; set; }

    public virtual Apartment Apartment { get; set; } = null!;

    public virtual ICollection<ElectricReading> ElectricReadings { get; set; } = new List<ElectricReading>();
}
