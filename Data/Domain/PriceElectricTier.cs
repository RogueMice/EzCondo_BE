using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class PriceElectricTier
{
    public Guid Id { get; set; }

    public decimal MinKWh { get; set; }

    public decimal MaxKWh { get; set; }

    public decimal PricePerKWh { get; set; }
}
