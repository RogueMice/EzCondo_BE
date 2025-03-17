using System;
using System.Collections.Generic;

namespace EzCondo_Data.Context;

public partial class Booking
{
    public Guid Id { get; set; }

    public Guid ServiceId { get; set; }

    public Guid UserId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Service Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
