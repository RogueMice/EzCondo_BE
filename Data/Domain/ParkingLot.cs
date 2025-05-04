using EzCondo_Data.Context;
using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class ParkingLot
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public virtual ICollection<ParkingLotDetail> ParkingLotDetails { get; set; } = new List<ParkingLotDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
