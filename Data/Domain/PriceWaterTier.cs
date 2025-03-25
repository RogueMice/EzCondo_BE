using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class PriceWaterTier
{
    public Guid Id { get; set; }

    public decimal PricePerM3 { get; set; }
}
