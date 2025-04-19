using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class ElectricBill
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid ReadingId { get; set; }

    public decimal TotalComsumption { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreateDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ElectricReading Reading { get; set; } = null!;
}
