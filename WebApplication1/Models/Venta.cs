using WebApplication1.Logica;

namespace WebApplication1.Models
{
    public class Venta
    {
        private List<Producto> productos = new List<Producto>();

        public Cliente? Cliente = new Cliente();
        public List<Producto> Productos { get { return productos; } }
        public Envio? Envio { get; set; }
        public int Id { get; set; }
        public double Total { get; set; }
        public int CantProductos { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoPago { get; set; }
        public string TipoVenta { get; set; }
        public int? IdEnvio { get; set; }
        public int? ClienteId { get; set; }
        public Venta(Carrito c, string tpago, string tventa, Envio envio, int clienteid)
        {
            productos = c.Productos;
            Total = c.Total;
            CantProductos = c.Productos.Count;
            Fecha = DateTime.Now;
            TipoPago = tpago;
            TipoVenta = tventa;
            this.Envio = envio;
            ClienteId = clienteid;
        }

        public Venta() { }
    }
}
