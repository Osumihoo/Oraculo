namespace Oraculo.Models
{
    public class BackupCostosPreciosExistencias
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal PiezasPorCaja { get; set; }
        public decimal ExistenciaEnCajas { get; set; }
        public decimal Costo { get; set; }
        public string Impuesto { get; set; }
        public string WhsCode { get; set; }
        public decimal IVA0IEPS8 { get; set; }
        public decimal IVA16 { get; set; }
        public decimal IVA16IEPS265 { get; set; }
        public decimal IVA16IEPS30 { get; set; }
        public decimal IVA16IEPS53 { get; set; }
        public decimal CostoConImpuestos { get; set; }
        public decimal CostoCaja { get; set; }
        public decimal Precio { get; set; }
        public decimal Ganancia { get; set; }
        public decimal MargenPorcentaje { get; set; }
    }
}
