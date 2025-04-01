using System;
using System.Collections.Generic;
using EzCondo_Data.Domain;

namespace EzCondo_Data.Context;

public partial class PasswordResetCode
{
    public Guid UserId { get; set; }

    public string Code { get; set; } = null!;

    public DateTime ExpireAt { get; set; }

    public virtual User User { get; set; } = null!;
}
