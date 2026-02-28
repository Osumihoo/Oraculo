namespace Oraculo.Models
{
    public class PriceList
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public decimal PrecioArtGDL { get; set; }
        public decimal PrecioCajaGDL { get; set; }

        public decimal PrecioArtCOL { get; set; }
        public decimal PrecioCajaCOL { get; set; }
    }
}
