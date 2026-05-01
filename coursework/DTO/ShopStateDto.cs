using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.DTO
{
    internal class ShopStateDto
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal CurrentRevenue { get; set; }
        public int CurrentQueue { get; set; }
        public double CongestionLevel { get; set; } 
        public double Attractiveness { get; set; }
    }
}
