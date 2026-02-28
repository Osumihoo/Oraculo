namespace Oraculo.Models
{
    public class WoocommercePriceUpdate
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string WhsCode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }
    }
}
