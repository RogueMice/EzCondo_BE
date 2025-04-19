using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class Apartment
{
    public Guid Id { get; set; }

    public string ApartmentNumber { get; set; } = null!;

    public int ResidentNumber { get; set; }

    public decimal Acreage { get; set; }

    public string? Description { get; set; }

    public Guid? UserId { get; set; }

    public virtual ElectricMeter? ElectricMeter { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<WaterMeter> WaterMeters { get; set; } = new List<WaterMeter>();
}
