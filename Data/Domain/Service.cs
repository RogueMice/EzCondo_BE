using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class Service
{
    public Guid Id { get; set; }

    public string ServiceName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool? TypeOfMonth { get; set; }

    public bool? TypeOfYear { get; set; }

    public decimal? PriceOfMonth { get; set; }

    public decimal? PriceOfYear { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<ServiceImage> ServiceImages { get; set; } = new List<ServiceImage>();
}
