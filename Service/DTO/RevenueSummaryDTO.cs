using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class RevenueSummaryDTO
    {
        public decimal TotalRevenue { get; set; }
        public int PercentChange { get; set; }
        public List<MonthlyRevenueDTO> MonthlyRevenue { get; set; }
    }
}
