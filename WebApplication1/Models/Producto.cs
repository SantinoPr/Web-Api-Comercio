using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Models
{
    public class Producto
    {
        public List<Imagen> Imagenes = new List<Imagen>();
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? Stock { get; set; }
        public double? PrecioCompra { get; set; }
        public double PrecioVenta { get; set; }
        public int? CategoriaId { get; set; }
        public string? CategoriaNom { get; set; }
        public string? Estado
        {
            get
            {
                if (Stock == 0)
                    return "No disponible";
                else
                    return "Disponible";
            }
        }

        public int stockSeleccionado { get; set; }

        // Sobrescribir Equals para comparar por Id
        public override bool Equals(object obj)
        {
            if (obj is Producto otroProducto)
            {
                return this.Id == otroProducto.Id; // Comparación por Id
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
