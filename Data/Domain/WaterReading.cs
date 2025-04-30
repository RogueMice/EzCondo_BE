using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class WaterReading
{
    public Guid Id { get; set; }

    public Guid WaterMetersId { get; set; }

    public DateTime? ReadingPreDate { get; set; }

    public DateTime ReadingCurrentDate { get; set; }

    public decimal PreWaterNumber { get; set; }

    public decimal CurrentWaterNumber { get; set; }

    public decimal Consumption { get; set; }

    public virtual ICollection<WaterBill> WaterBills { get; set; } = new List<WaterBill>();

    public virtual WaterMeter WaterMeters { get; set; } = null!;
}
