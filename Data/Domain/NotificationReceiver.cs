using System;
using System.Collections.Generic;
using EzCondo_Data.Domain;

namespace EzCondo_Data.Context;

public partial class NotificationReceiver
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }

    public Guid? UserId { get; set; }

    public string? Role { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual User? User { get; set; }
}
