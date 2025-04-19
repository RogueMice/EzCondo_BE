using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class ParkingLot
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool Checking { get; set; }

    public Guid? UserId { get; set; }

    public virtual User? User { get; set; }
}
