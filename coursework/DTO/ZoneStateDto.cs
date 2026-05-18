using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.DTO
{
    public class ZoneStateDto
    {
        public Guid ZoneId { get; set; }
        public string ZoneName { get; set; }
        public string Theme { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CurrentVisitors { get; set; }
        public double OccupancyRate { get; set; } 
        public List<ShopStateDto> ShopsData { get; set; } = new List<ShopStateDto>();
    }
}

