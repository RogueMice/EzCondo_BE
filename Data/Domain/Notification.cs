using System;
using System.Collections.Generic;
using EzCondo_Data.Context;

namespace EzCondo_Data.Domain;

public partial class Notification
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Type { get; set; } = null!;

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<NotificationImage> NotificationImages { get; set; } = new List<NotificationImage>();

    public virtual ICollection<NotificationReceiver> NotificationReceivers { get; set; } = new List<NotificationReceiver>();
}
