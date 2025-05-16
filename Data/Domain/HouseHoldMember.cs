using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class HouseHoldMember
{
    public Guid Id { get; set; }

    public string No { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Relationship { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual User User { get; set; } = null!;
}
