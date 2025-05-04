using System;
using System.Collections.Generic;
using EzCondo_Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace EzCondo_Data.Context;

public partial class ParkingLotDetail
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool Checking { get; set; }

    public Guid? ParkingLotId { get; set; }

    public virtual ParkingLot? ParkingLot { get; set; }
}
