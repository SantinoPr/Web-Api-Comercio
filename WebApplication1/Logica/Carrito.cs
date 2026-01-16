using WebApplication1.Models;

namespace WebApplication1.Logica
{
    public class Carrito
    {
        List<Producto> productos = new List<Producto>();
        double total = 0;
        double cantProductos = 0;

        public List<Producto> Productos
        {
            get
            {
                return productos;
            }
        }
        public double Total
        {
            get
            {
                total = productos.Sum(p => p.PrecioVenta * p.stockSeleccionado);
                return total;
            }
        }
        public double TotalCompra
        {
            get
            {
                total = Convert.ToDouble(productos.Sum(p => p.PrecioCompra * p.stockSeleccionado));
                return total;
            }
        }
    }
}
