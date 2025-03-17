using System;
using System.Collections.Generic;

namespace EzCondo_Data.Context;

public partial class ParkingLot
{
    public Guid Id { get; set; }

    public string Spot { get; set; } = null!;

    public Guid ServiceId { get; set; }

    public Guid? UserId { get; set; }

    public virtual Service Service { get; set; } = null!;

    public virtual User? User { get; set; }
}
