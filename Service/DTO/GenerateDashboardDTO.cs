using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class GenerateDashboardDTO
    {
        public double Total { get; set; }

        public int Increase { get; set; }

        public double GrowthRatePercent { get; set; }

        public string TrendDescription { get; set; }
    }
}
