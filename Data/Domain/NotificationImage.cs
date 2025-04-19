using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class NotificationImage
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }

    public string? ImagePath { get; set; }

    public virtual Notification Notification { get; set; } = null!;
}
