using System;
using System.Collections.Generic;

namespace EzCondo_Data.Domain;

public partial class PriceParkingLot
{
    public Guid Id { get; set; }

    public decimal? PricePerMotor { get; set; }

    public decimal? PricePerOto { get; set; }
}
