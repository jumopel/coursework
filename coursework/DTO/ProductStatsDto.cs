using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.DTO
{
    public class ProductStatsDto
    {
        public string ShopName { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue => Price * SalesCount;
    }
}
