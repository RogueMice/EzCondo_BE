using System;
using System.Collections.Generic;

namespace EzCondo_Data.Context;

public partial class UserDevice
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string FcmToken { get; set; } = null!;

    public bool IsActive { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
