using Npgsql;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class VentaData
    {
        private readonly string _connectionString;

        public VentaData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        private NpgsqlConnection DbConnection() => new NpgsqlConnection(_connectionString);

        public async Task<IEnumerable<Venta>> GetAllVentas()
        {
            List<Venta> ventas = new List<Venta>();

            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                NpgsqlCommand cmd = new NpgsqlCommand("select * from GetAllVentas()", db);
                cmd.CommandType = System.Data.CommandType.Text;

                using (NpgsqlDataReader rd = await cmd.ExecuteReaderAsync())
                {
                    while (rd.Read())
                    {
                        Venta venta = new Venta
                        {

                            Id = Convert.ToInt32(rd["ven_id"]),
                            Total = Convert.ToDouble(rd["ven_total"]),
                            CantProductos = Convert.ToInt32(rd["ven_cant_productos"]),
                            Fecha = Convert.ToDateTime(rd["ven_fecha"]),
                            TipoPago = rd["tipo_pago_descripcion"].ToString(),
                            TipoVenta = rd["tipo_venta_descripcion"].ToString(),

                            // Comprobar si el valor es DBNull antes de convertirlo
                            IdEnvio = rd["ven_envio_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["ven_envio_id"]),
                            ClienteId = rd["ven_cliente_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["ven_cliente_id"])

                        };
                        venta.Cliente.Id = Convert.ToInt32(rd["ven_cliente_id"]);
                        venta.Cliente.Nombre = rd["cliente_nombre"].ToString();
                        ventas.Add(venta);

                    }

                }

            }
            return ventas;

        }


        public async Task<Venta> GetVentaById(int id)
        {
            Venta venta = null;

            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                // Definir la consulta SQL para obtener la venta por ID
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM venta where ven_id=@id", db);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.CommandType = System.Data.CommandType.Text;

                using (NpgsqlDataReader rd = await cmd.ExecuteReaderAsync())
                {
                    if (rd.Read())
                    {
                        // Mapear los datos al objeto Venta
                        venta = new Venta
                        {
                            Id = Convert.ToInt32(rd["ven_id"]),
                            Total = Convert.ToDouble(rd["ven_total"]),
                            CantProductos = Convert.ToInt32(rd["ven_cant_productos"]),
                            Fecha = Convert.ToDateTime(rd["ven_fecha"]),
                            TipoPago = ((int)rd["ven_tipo_pago_id"] - 1).ToString(),
                            TipoVenta = ((int)rd["ven_tipo_venta_id"] - 1).ToString(),

                            // Manejo de valores nulos
                            IdEnvio = rd["ven_envio_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["ven_envio_id"]),
                            ClienteId = rd["ven_cliente_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["ven_cliente_id"])
                        };


                    }
                }
                NpgsqlCommand cmdVentaProducto = new NpgsqlCommand("select p.prod_id, p.prod_stock, p.prod_nombre, SUM(v.stock) as total_stock, v.precio_venta_producto" +
                    " from producto p" +
                    " join venta_producto as v on v.id_producto = p.prod_id" +
                    " where v.id_venta=@id" +
                    " group by p.prod_id, p.prod_nombre, v.precio_venta_producto", db);
                cmdVentaProducto.Parameters.AddWithValue("@id", id);
                cmdVentaProducto.CommandType = System.Data.CommandType.Text;
                using (NpgsqlDataReader rdVentaProducto = await cmdVentaProducto.ExecuteReaderAsync())
                {
                    while (rdVentaProducto.Read())
                    {
                        // Mapear los datos al objeto Producto
                        Producto producto = new Producto
                        {
                            Id = Convert.ToInt32(rdVentaProducto["prod_id"]),
                            Stock = Convert.ToInt32(rdVentaProducto["prod_stock"]),
                            Nombre = rdVentaProducto["prod_nombre"].ToString(),
                            stockSeleccionado = Convert.ToInt32(rdVentaProducto["total_stock"]),
                            PrecioVenta = Convert.ToDouble(rdVentaProducto["precio_venta_producto"])
                        };
                        venta.Productos.Add(producto);
                    }
                }

                //compruebo si tiene algun envio
                if (venta.IdEnvio != null)
                {
                    EnvioData envioData = new EnvioData(_connectionString);
                    Envio envio = await envioData.GetEnvioById((int)venta.IdEnvio);
                    venta.Envio = envio;
                }

            }
            return venta;
        }


        public async Task<bool> DeleteVenta(int id)
        {

            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("deleteVenta", db);
                    cmd.Parameters.AddWithValue(@"id_param", id);


                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();

                    string query = "delete from ingreso_gasto where ing_id_entidad = @id_venta;";

                    NpgsqlCommand cmdFlujoCaja = new NpgsqlCommand(query, db);

                    cmdFlujoCaja.Parameters.AddWithValue("@id_venta", id);

                    await cmdFlujoCaja.ExecuteNonQueryAsync();
                    Console.WriteLine("eliminacion exitosa");
                    return true;

                }
                catch (NpgsqlException e)
                {
                }
            }
            return false;

        }

        public async Task<bool> AddVenta(Venta venta)
        {

            int? id = null;
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                //try
                //{
                if (venta.Envio != null)
                {
                    EnvioData envioData = new EnvioData(_connectionString);
                    venta.IdEnvio = await envioData.AddEnvio(venta.Envio);
                }
                //agregar la venta principal
                using (var cmd = new NpgsqlCommand("select * from addventa(@p_total,@p_cantProductos,@p_fecha,@p_tipoP, @p_tipoV,@p_envio, @p_cliente)", db))
                {

                    cmd.Parameters.AddWithValue("@p_total", venta.Total);
                    cmd.Parameters.AddWithValue("@p_cantProductos", venta.CantProductos);
                    cmd.Parameters.AddWithValue("@p_fecha", (DateTime)venta.Fecha);
                    cmd.Parameters.AddWithValue("@p_tipoP", Convert.ToInt32(venta.TipoPago));
                    cmd.Parameters.AddWithValue("@p_tipoV", Convert.ToInt32(venta.TipoVenta));
                    cmd.Parameters.AddWithValue("@p_envio", venta.IdEnvio != null ? (int)venta.IdEnvio : DBNull.Value);
                    cmd.Parameters.AddWithValue("@p_cliente", venta.ClienteId);
                    cmd.CommandType = System.Data.CommandType.Text;
                    venta.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                //agregar ventas_producto

                foreach (Producto p in venta.Productos)
                {
                    NpgsqlCommand cmdVentaProducto = new NpgsqlCommand("procesar_venta", db);
                    cmdVentaProducto.Parameters.AddWithValue(@"p_id_venta", venta.Id);
                    cmdVentaProducto.Parameters.AddWithValue(@"p_id_producto", p.Id);
                    cmdVentaProducto.Parameters.AddWithValue(@"p_cantidad", p.stockSeleccionado);
                    cmdVentaProducto.Parameters.AddWithValue(@"p_precio_venta", p.PrecioVenta);
                    cmdVentaProducto.CommandType = System.Data.CommandType.StoredProcedure;

                    await cmdVentaProducto.ExecuteNonQueryAsync();
                }
                Console.WriteLine("Registro exitoso");

                //generar el ingreso en flujo de caja
                string query = "insert into ingreso_gasto (ing_tipo_ingreso_gasto_id, ing_es_ingreso, ing_fecha,ing_id_entidad, ing_monto)" +
                    " select 8, true, CURRENT_DATE, @id_venta, sum(ganancia) from venta_producto where id_venta = @id_venta;";

                NpgsqlCommand cmdFlujoCaja = new NpgsqlCommand(query, db);

                cmdFlujoCaja.Parameters.AddWithValue("@id_venta", venta.Id);

                await cmdFlujoCaja.ExecuteNonQueryAsync();


                return true;

                //}
                //catch (NpgsqlException e)
                //{
                //MessageBox.Show(e.Message);
                //}
            }

            return false;
        }
        public async Task<bool> setVenta(Venta venta)
        {
            int? id = null;
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                try
                {
                    if (venta.Envio != null)
                    {
                        EnvioData envioData = new EnvioData(_connectionString);
                        id = await envioData.AddEnvio(venta.Envio);
                    }
                    NpgsqlCommand cmd = new NpgsqlCommand("setventa", db);
                    cmd.Parameters.AddWithValue(@"id", venta.Id);
                    cmd.Parameters.AddWithValue(@"total", venta.Total);
                    cmd.Parameters.AddWithValue(@"cantproductos", venta.CantProductos);
                    cmd.Parameters.AddWithValue(@"fecha", venta.Fecha);
                    cmd.Parameters.AddWithValue(@"tipop", Convert.ToInt32(venta.TipoPago));
                    cmd.Parameters.AddWithValue(@"tipov", Convert.ToInt32(venta.TipoVenta));
                    cmd.Parameters.AddWithValue(@"envio", id.HasValue ? (object)id.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue(@"cliente", venta.ClienteId);

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    await cmd.ExecuteNonQueryAsync();


                    foreach (Producto p in venta.Productos)
                    {
                        NpgsqlCommand cmdVentaProducto = new NpgsqlCommand("procesar_venta", db);
                        cmdVentaProducto.Parameters.AddWithValue(@"p_id_venta", venta.Id);
                        cmdVentaProducto.Parameters.AddWithValue(@"p_id_producto", p.Id);
                        cmdVentaProducto.Parameters.AddWithValue(@"p_cantidad", p.stockSeleccionado);
                        cmdVentaProducto.Parameters.AddWithValue(@"p_precio_venta", p.PrecioVenta);
                        cmdVentaProducto.CommandType = System.Data.CommandType.StoredProcedure;

                        await cmdVentaProducto.ExecuteNonQueryAsync();
                    }
                    Console.WriteLine("edicion exitosa");

                    //generar el ingreso en flujo de caja
                    string query = "delete from ingreso_gasto where ing_id_entidad = @id_venta;" +
                        "insert into ingreso_gasto (ing_tipo_ingreso_gasto_id, ing_es_ingreso, ing_fecha,ing_id_entidad, ing_monto)" +
                        " select 8, true, CURRENT_DATE, @id_venta, sum(ganancia) from venta_producto where id_venta = @id_venta;";

                    NpgsqlCommand cmdFlujoCaja = new NpgsqlCommand(query, db);

                    cmdFlujoCaja.Parameters.AddWithValue("@id_venta", venta.Id);

                    await cmdFlujoCaja.ExecuteNonQueryAsync();
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
