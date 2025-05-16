using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class HouseHoldMemberDashBoardDTO
    {
        public int TotalResidents { get; set; }               // Tổng số cư dân hiện tại

        public double GrowthRatePercent { get; set; }         // Tỷ lệ % tăng/giảm so với tuần trước (VD: +12.5 hoặc -8.3)

        public string TrendDescription { get; set; }    // Mô tả: "Tăng so với tuần trước" hoặc "Giảm so với tuần trước"
    }
}
