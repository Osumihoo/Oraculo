namespace Oraculo.Models
{
    public class CostList
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public decimal CostoSinImpuestos { get; set; }
        public decimal UMI { get; set; }

        public decimal CostoConImpuestos { get; set; }
        public decimal CostoCaja { get; set; }
    }
}
