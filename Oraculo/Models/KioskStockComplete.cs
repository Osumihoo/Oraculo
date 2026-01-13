namespace Oraculo.Models
{
    public class KioskStockComplete
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal OnHand { get; set; }
        public decimal NumInSale { get; set; }
        public decimal Price { get; set; }
        public string ItmsGrpCod { get; set; }
    }
}
