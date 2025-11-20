namespace Oraculo.Models
{
    public class BranchManagersLast45Days
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Familia { get; set; }
        public decimal CedisAbastos15Dias { get; set; }
        public decimal CedisAbastos30Dias { get; set; }
        public decimal CedisAbastos30DiasPromedio { get; set; }
        public decimal StockCedis { get; set; }
        public decimal ExistenciaCorporativo { get; set; }
        public decimal Existencia14 { get; set; }
        public decimal ExistenciaMandarina { get; set; }  
        public decimal TotalStock { get; set; }
        public decimal DiferenciaMax30StockTotal { get; set; }
    }
}
