namespace Oraculo.Models
{
    public class Response
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Response<T>
    {
        public int Code { get; set; }           // Código de estado (ej. 200, 500)
        public string Description { get; set; } // Mensaje descriptivo
        public T Data { get; set; }             // Datos que deseas devolver
    }
}
