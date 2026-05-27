namespace coursework.DTO
{
    public class ProductStatsDto
    {
        public string ShopName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue => Price * SalesCount;

        public decimal CostPrice { get; set; }
        public decimal TotalProfit => (Price - CostPrice) * SalesCount; 
        public double MarginPercent => Price > 0 ? (double)((Price - CostPrice) / Price) * 100 : 0;
    }
}