using WebApplication1.Models;
using Npgsql;

namespace WebApplication1.Data
{
    public class ProductoData
    {
        public readonly string _connectionString;

        public ProductoData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        private NpgsqlConnection DbConnection() => new NpgsqlConnection(_connectionString);

        public async Task<IEnumerable<Producto>> GetAllProductos()
        {
            List<Producto> productos = new List<Producto>();

            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM getallproductos()", db);
                cmd.CommandType = System.Data.CommandType.Text;

                using (NpgsqlDataReader rd = await cmd.ExecuteReaderAsync())
                {
                    while (rd.Read())
                    {
                        Producto producto = new Producto
                        {
                            Id = Convert.ToInt32(rd["prod_id"]),
                            Nombre = rd["prod_nombre"].ToString(),
                            Stock = rd["prod_stock"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["prod_stock"]),
                            PrecioVenta = Convert.ToDouble(rd["prod_precio_venta"]),
                            CategoriaNom = rd["categoria_descripcion"] == DBNull.Value ? (string?)null : rd["categoria_descripcion"].ToString(),

                        };
                        productos.Add(producto);
                    }
                }
               
            }
            return productos;
        }

        public async Task<Producto> GetProductoById(int id)
        {
            Producto producto = null;

            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM getproductobyid(@id)", db);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.CommandType = System.Data.CommandType.Text;

                using (NpgsqlDataReader rd = await cmd.ExecuteReaderAsync())
                {
                    if (rd.Read())
                    {
                        producto = new Producto
                        {
                            Id = Convert.ToInt32(rd["prod_id"]),
                            Nombre = rd["prod_nombre"].ToString(),
                            Stock = rd["prod_stock"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["prod_stock"]),
                            PrecioVenta = Convert.ToDouble(rd["prod_precio_venta"]),
                            CategoriaId = (int)(rd["prod_categoria_id"] == DBNull.Value ? null : (int?)rd["prod_categoria_id"]),
                            CategoriaNom = rd["categoria_descripcion"] == DBNull.Value ? (string?)null : rd["categoria_descripcion"].ToString()

                        };
                    }
                }
            }
            return producto;
        }

        public async Task<IEnumerable<Producto>> GetAllProductosEdit(int idVenta)//que desastre, lo hago para obtener la lista de producto + stock seleccionado en edicion
        {
            List<Producto> productos = new List<Producto>();

            using (NpgsqlConnection db = DbConnection())
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select p.prod_id, p.prod_nombre, p.prod_precio_venta, p.prod_stock + COALESCE(v.stock, 0) as stock " +
                                        "from producto as p " +
                                        "left join venta_producto as v on p.prod_id = v.id_producto and v.id_venta = @id " +
                                        "order by p.prod_id", db);
                cmd.Parameters.AddWithValue("@id", idVenta);
                cmd.CommandType = System.Data.CommandType.Text;

                using (NpgsqlDataReader rd = await cmd.ExecuteReaderAsync())
                {
                    while (rd.Read())
                    {
                        Producto producto = new Producto
                        {
                            Id = Convert.ToInt32(rd["prod_id"]),
                            Nombre = rd["prod_nombre"].ToString(),
                            Stock = rd["stock"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["stock"]),
                            PrecioVenta = Convert.ToDouble(rd["prod_precio_venta"]),
                        };
                        productos.Add(producto);
                    }
                }
            }
            return productos;
        }


        public async Task<int> AddProducto(Producto producto)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();
                //try
                //{
                NpgsqlCommand cmd = new NpgsqlCommand("select * from addproducto(@nombre, @precioventa, @categoriaid)", db);
                cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@precioventa", producto.PrecioVenta);
                cmd.Parameters.AddWithValue("@categoriaid", producto.CategoriaId);
                cmd.CommandType = System.Data.CommandType.Text;
                int idProducto = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                Console.WriteLine("Producto registrado exitosamente");
                return idProducto;
                //}
                //catch (NpgsqlException e)
                //{
                //    MessageBox.Show(e.Message);
                //}
            }
            //return -1;
        }

        public async Task<bool> SetProducto(Producto producto)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();
                NpgsqlCommand cmd = new NpgsqlCommand("setproducto", db);
                cmd.Parameters.AddWithValue(@"id", producto.Id);
                cmd.Parameters.AddWithValue(@"nombre", producto.Nombre);
                cmd.Parameters.AddWithValue(@"precioventa", producto.PrecioVenta);
                cmd.Parameters.AddWithValue(@"categoriaid", producto.CategoriaId);


                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await cmd.ExecuteNonQueryAsync();

                Console.WriteLine("Producto actualizado exitosamente");
                return true;


            }
            return false;
        }

        //metodo creado exclusivamente para cuando se genere una compra desde la web o se vea afectado por eliminarse de una venta
        public async Task<bool> SetProductoStock(Producto producto)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                NpgsqlCommand cmd = new NpgsqlCommand("update producto set prod_stock = @stock where prod_id=@id", db);
                cmd.Parameters.AddWithValue(@"id", producto.Id);
                cmd.Parameters.AddWithValue(@"stock", producto.Stock);


                cmd.CommandType = System.Data.CommandType.Text;
                await cmd.ExecuteNonQueryAsync();

                Console.WriteLine("Producto actualizado exitosamente");
                return true;


            }
            return false;
        }


        public async Task<bool> DeleteProducto(int id)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("deleteProducto", db);
                    cmd.Parameters.AddWithValue(@"id_param", id);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine("Producto eliminado exitosamente");
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
