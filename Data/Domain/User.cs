using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? Avatar { get; set; }

    public int RoleId { get; set; }

    public Guid? TokenVersion { get; set; }

    public virtual ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Citizen? Citizen { get; set; }

    public virtual ICollection<ElectricBill> ElectricBills { get; set; } = new List<ElectricBill>();

    public virtual ICollection<HouseHoldMember> HouseHoldMembers { get; set; } = new List<HouseHoldMember>();

    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public virtual ICollection<NotificationReceiver> NotificationReceivers { get; set; } = new List<NotificationReceiver>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<ParkingLot> ParkingLots { get; set; } = new List<ParkingLot>();

    public virtual PasswordResetCode? PasswordResetCode { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();

    public virtual ICollection<WaterBill> WaterBills { get; set; } = new List<WaterBill>();
}
