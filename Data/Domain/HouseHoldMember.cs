using System;
using System.Collections.Generic;
using EzCondo_Data.Domain;

namespace EzCondo_Data.Context;

public partial class HouseHoldMember
{
    public Guid Id { get; set; }

    public string No { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Relationship { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
