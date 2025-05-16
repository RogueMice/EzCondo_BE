using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public  class OtherService
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Price { get; set; }

    public string? Description { get; set; }
}
