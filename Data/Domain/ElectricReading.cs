using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class ElectricReading
{
    public Guid Id { get; set; }

    public Guid ElectricMetersId { get; set; }

    public DateTime? ReadingPreDate { get; set; }

    public DateTime? ReadingCurrentDate { get; set; }

    public decimal PreElectricNumber { get; set; }

    public decimal CurrentElectricNumber { get; set; }

    public decimal Consumption { get; set; }

    public virtual ICollection<ElectricBill> ElectricBills { get; set; } = new List<ElectricBill>();

    public virtual ElectricMeter ElectricMeters { get; set; } = null!;
}
