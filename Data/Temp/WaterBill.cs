using System;
using System.Collections.Generic;

namespace EzCondo_Data.Temp;

public partial class WaterBill
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid ReadingId { get; set; }

    public decimal TotalConsumption { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreateDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual WaterReading Reading { get; set; } = null!;
}
