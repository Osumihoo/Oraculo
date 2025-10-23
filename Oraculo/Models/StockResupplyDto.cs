namespace Oraculo.Models
{
    public class StockResupplyDto
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Sucursal { get; set; }
        public decimal Resurtir { get; set; }
        public decimal StockSucursal { get; set; }
        public decimal Minimo { get; set; }
        public decimal Maximo { get; set; }
        public DateTime UltimaVezBajoMinimo { get; set; }
        public int UltimoDiaArribaMinimo { get; set; }

        // Dinámico: aquí se meten Stock300, Stock301, etc.
        public Dictionary<string, decimal> StocksPorAlmacen { get; set; } = new();
    }
}
