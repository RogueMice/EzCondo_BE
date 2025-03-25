using System;
using System.Collections.Generic;

namespace EzCondo_Data.Temp;

public partial class PasswordResetCode
{
    public Guid UserId { get; set; }

    public string Code { get; set; } = null!;

    public DateTime ExpireAt { get; set; }

    public virtual User User { get; set; } = null!;
}
