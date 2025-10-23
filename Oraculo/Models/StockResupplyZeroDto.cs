namespace Oraculo.Models
{
    public class StockResupplyZeroDto
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public Dictionary<string, decimal> StockByWarehouse { get; set; } = new Dictionary<string, decimal>();
    }
}
