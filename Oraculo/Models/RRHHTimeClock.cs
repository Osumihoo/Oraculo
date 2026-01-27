namespace Oraculo.Models
{
    public class RRHHTimeClock
    {
        public string Sucursal { get; set; }
        public string? SucursalChecada { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime? Fecha { get; set; }
        public string Entrada { get; set; }
        public string Salida { get; set; }
        public decimal? HorasTrabajadas { get; set; }
        public int HoraDeComida { get; set; }
    }
}
