using Npgsql;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ImagenData
    {
        private readonly string _connectionString;

        public ImagenData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public ImagenData(string configuration)
        {
            _connectionString = configuration;
        }
        private NpgsqlConnection DbConnection() => new NpgsqlConnection(_connectionString);
        // Obtener todas las imágenes asociadas a un producto
        public async Task<IEnumerable<Imagen>> GetImagenesPorProducto(int productoId)
        {
            List<Imagen> imagenes = new List<Imagen>();

            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();
                NpgsqlCommand cmd = new NpgsqlCommand("select * from imagen where img_prod_id=@id", db);
                cmd.Parameters.AddWithValue("@id", productoId);
                cmd.CommandType = System.Data.CommandType.Text;

                using (NpgsqlDataReader rd = await cmd.ExecuteReaderAsync())
                {
                    while (rd.Read())
                    {
                        Imagen imagen = new Imagen
                        {
                            Id = Convert.ToInt32(rd["img_id"]),
                            ImgPath = rd["img_path"].ToString(),
                            ProductoId = Convert.ToInt32(rd["img_prod_id"])
                        };
                        imagenes.Add(imagen);
                    }
                }
            }

            return imagenes;
        }

        // Agregar una nueva imagen a un producto
        public async Task<bool> AddImagen(Imagen imagen)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                NpgsqlCommand cmd = new NpgsqlCommand("saveimages", db);
                cmd.Parameters.AddWithValue("@path", imagen.ImgPath);
                cmd.Parameters.AddWithValue("@prod_id", imagen.ProductoId);

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await cmd.ExecuteNonQueryAsync();

                return true;

            }

            return false;
        }

        // Eliminar una imagen por ID
        public async Task<bool> DeleteImagen(int id)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM imagen WHERE img_id = @id", db);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.CommandType = System.Data.CommandType.Text;
                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine("Imagen eliminada exitosamente");
                    return true;
                }
                catch (NpgsqlException e)
                {
                }
            }

            return false;
        }

        // Actualizar la ruta de la imagen
        public async Task<bool> UpdateImagen(Imagen imagen)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("UPDATE imagen SET img_path = @imgPath WHERE img_id = @id", db);
                    cmd.Parameters.AddWithValue("@imgPath", imagen.ImgPath);
                    cmd.Parameters.AddWithValue("@id", imagen.Id);

                    cmd.CommandType = System.Data.CommandType.Text;
                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine("Imagen actualizada exitosamente");
                    return true;
                }
                catch (NpgsqlException e)
                {
                }
            }

            return false;
        }
    }
}
