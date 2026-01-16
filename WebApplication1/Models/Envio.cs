namespace WebApplication1.Models
{
    public class Envio
    {
        public int EnvioId { get; set; }        // Identificador del envío
        public string? Direccion { get; set; }   // Dirección de destino del envío
        public string? Ciudad { get; set; }      // Ciudad de destino
        public string? Provincia { get; set; }   // Provincia de destino
        public string? Cd { get; set; }          // Código postal del destino
        public double? Costo { get; set; }        // Costo del envío
        public DateTime? FechaHora { get; set; } // Fecha y hora del envío

        // Constructor vacío
        public Envio() { }

        // Constructor con parámetros
        public Envio(int envioId, string direccion, string ciudad, string provincia, string cd, float costo, DateTime fechaHora)
        {
            EnvioId = envioId;
            Direccion = direccion;
            Ciudad = ciudad;
            Provincia = provincia;
            Cd = cd;
            Costo = costo;
            FechaHora = fechaHora;
        }
    }
}
