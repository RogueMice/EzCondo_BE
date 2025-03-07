using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid ReciverId { get; set; }

    public Guid SenderId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime SentAt { get; set; }

    public virtual User Reciver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
