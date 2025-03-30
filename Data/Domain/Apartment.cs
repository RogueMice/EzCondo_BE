using System;
using System.Collections.Generic;

namespace EzCondo_Data.Context;

public partial class Apartment
{
    public Guid Id { get; set; }

    public string ApartmentNumber { get; set; } = null!;

    public int ResidentNumber { get; set; }

    public decimal Acreage { get; set; }

    public string? Description { get; set; }

    public Guid? UserId { get; set; }

    public virtual ICollection<ElectricMeter> ElectricMeters { get; set; } = new List<ElectricMeter>();

    public virtual User? User { get; set; }

    public virtual ICollection<WaterMeter> WaterMeters { get; set; } = new List<WaterMeter>();
}
