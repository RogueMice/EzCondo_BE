using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? BookingId { get; set; }

    public Guid? ElectricBillId { get; set; }

    public Guid? WaterBillId { get; set; }

    public decimal Amount { get; set; }

    public string? TransactionId { get; set; }

    public string Status { get; set; } = null!;

    public string Method { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public Guid? ParkingId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ElectricBill? ElectricBill { get; set; }

    public virtual ParkingLot? Parking { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual WaterBill? WaterBill { get; set; }
}
