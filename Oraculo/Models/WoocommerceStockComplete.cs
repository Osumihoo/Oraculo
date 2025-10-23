namespace Oraculo.Models
{
    public class WoocommerceStockComplete
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string WhsCode { get; set; }
        public string ItmsGrpNam { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal BoxPrice { get; set; }
        public decimal Stock { get; set; }
        public decimal NumInSale { get; set; }
        public int EsRET { get; set; }
        public decimal SWeight1 { get; set; }
        public decimal SWidth1 { get; set; }
        public decimal SHeight1 { get; set; }
        public decimal SLength1 { get; set; }
        public decimal SVolume { get; set; }

    }
}
