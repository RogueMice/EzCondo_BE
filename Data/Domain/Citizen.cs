using System;
using System.Collections.Generic;

namespace EzCondo_Data.Context;

public partial class Citizen
{
    public Guid UserId { get; set; }

    public string No { get; set; } = null!;

    public DateOnly DateOfIssue { get; set; }

    public DateOnly? DateOfExpiry { get; set; }

    public string? FrontImage { get; set; }

    public string? BackImage { get; set; }

    public virtual User User { get; set; } = null!;
}
