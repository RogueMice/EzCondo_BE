using System;
using System.Collections.Generic;

namespace EzCondo_Data.Context;

public partial class ServiceImage
{
    public Guid Id { get; set; }

    public Guid ServiceId { get; set; }

    public string ImgPath { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
