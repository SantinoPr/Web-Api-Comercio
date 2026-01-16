using Npgsql;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class EnvioData
    {
        private readonly string _connectionString;

        public EnvioData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public EnvioData(string configuration)
        {
            _connectionString = configuration;
        }
        private NpgsqlConnection DbConnection() => new NpgsqlConnection(_connectionString);

        public async Task<IEnumerable<Envio>> GetAllEnvios()
        {
            List<Envio> envios = new List<Envio>();

            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM envio", db);
                cmd.CommandType = CommandType.Text;

                using (NpgsqlDataReader rd = await cmd.ExecuteReaderAsync())
                {
                    while (rd.Read())
                    {
                        Envio envio = new Envio
                        {
                            EnvioId = Convert.ToInt32(rd["envio_id"]),
                            Direccion = rd["envio_direccion"] as string, // Puede ser nulo
                            Ciudad = rd["envio_ciudad"] as string, // Puede ser nulo
                            Provincia = rd["envio_provincia"] as string, // Puede ser nulo
                            Cd = rd["envio_cd"] as string,
                            Costo = Convert.ToSingle(rd["envio_costo"]),
                            FechaHora = Convert.ToDateTime(rd["envio_fecha_hora"])
                        };
                        envios.Add(envio);
                    }
                }
            }

            return envios;
        }

        public async Task<Envio> GetEnvioById(int id)
        {
            Envio envio = null;

            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM envio WHERE envio_id = @id_param", db);
                cmd.Parameters.AddWithValue("@id_param", id);
                cmd.CommandType = CommandType.Text;

                using (NpgsqlDataReader rd = await cmd.ExecuteReaderAsync())
                {
                    if (rd.Read())
                    {
                        envio = new Envio
                        {
                            EnvioId = Convert.ToInt32(rd["envio_id"]),
                            Direccion = rd["envio_direccion"] == DBNull.Value ? (string?)null : rd["envio_direccion"].ToString(),
                            Ciudad = rd["envio_ciudad"] == DBNull.Value ? (string?)null : rd["envio_ciudad"].ToString(),
                            Provincia = rd["envio_provincia"] == DBNull.Value ? (string?)null : rd["envio_provincia"].ToString(),
                            Cd = rd["envio_cd"] == DBNull.Value ? (string?)null : rd["envio_cd"].ToString(),
                            Costo = rd["envio_costo"] == DBNull.Value ? (double?)null : Convert.ToSingle(rd["envio_costo"]),
                            FechaHora = rd["envio_fecha_hora"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["envio_fecha_hora"])
                        };
                    }
                }
            }

            return envio;
        }

        public async Task<int> AddEnvio(Envio envio)
        {
            int id = 0;
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO envio (envio_direccion, envio_ciudad, envio_provincia, envio_cd, envio_costo, envio_fecha_hora)" +
                        " VALUES (@direccion, @ciudad, @provincia, @cd, @costo, @fecha_hora)", db);
                    cmd.Parameters.AddWithValue("@direccion", !string.IsNullOrEmpty(envio.Direccion) ? envio.Direccion : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ciudad", !string.IsNullOrEmpty(envio.Ciudad) ? envio.Ciudad : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@provincia", !string.IsNullOrEmpty(envio.Provincia) ? envio.Provincia : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@cd", !string.IsNullOrEmpty(envio.Cd) ? envio.Cd : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@costo", envio.Costo != null ? envio.Costo : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@fecha_hora", envio.FechaHora != null ? envio.FechaHora : (object)DBNull.Value);

                    cmd.CommandType = CommandType.Text;
                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine("Registro de envío exitoso");



                    NpgsqlCommand cmdId = new NpgsqlCommand("select max(envio_id) as id from envio", db);
                    cmdId.CommandType = System.Data.CommandType.Text;

                    using (NpgsqlDataReader rd = await cmdId.ExecuteReaderAsync())
                    {
                        while (rd.Read())
                        {
                            id = Convert.ToInt32(rd["id"]);

                        }
                    }
                    //generar el ingreso en flujo de caja
                    string query = "insert into ingreso_gasto (ing_tipo_ingreso_gasto_id, ing_es_ingreso, ing_fecha,ing_id_entidad, ing_monto)" +
                        " select 10, true, CURRENT_DATE, @id_venta, sum(ganancia) from venta_producto where id_venta = @id_venta;";

                    NpgsqlCommand cmdFlujoCaja = new NpgsqlCommand(query, db);

                    cmdFlujoCaja.Parameters.AddWithValue("@id_envio", id);
                    await cmdFlujoCaja.ExecuteNonQueryAsync();

                }
                catch (NpgsqlException e)
                {
                }
            }

            return id;
        }

        public async Task<bool> DeleteEnvio(int id)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM envio WHERE envio_id = @id_param", db);
                    cmd.Parameters.AddWithValue("@id_param", id);

                    cmd.CommandType = CommandType.Text;
                    await cmd.ExecuteNonQueryAsync();

                    string query = "delete from ingreso_gasto where ing_id_entidad = @id_envio;";

                    NpgsqlCommand cmdFlujoCaja = new NpgsqlCommand(query, db);

                    cmdFlujoCaja.Parameters.AddWithValue("@id_envio", id);

                    await cmdFlujoCaja.ExecuteNonQueryAsync();

                    Console.WriteLine("Eliminación de envío exitosa");
                    return true;
                }
                catch (NpgsqlException e)
                {
                }
            }

            return false;
        }

        public async Task<bool> UpdateEnvio(Envio envio)
        {
            using (NpgsqlConnection db = DbConnection())
            {
                await db.OpenAsync();

                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("UPDATE envio SET envio_direccion = @direccion, envio_ciudad = @ciudad, envio_provincia = @provincia, envio_cd = @cd, envio_costo = @costo, envio_fecha_hora = @fecha_hora WHERE envio_id = @id", db);
                    cmd.Parameters.AddWithValue("@id", envio.EnvioId);
                    cmd.Parameters.AddWithValue("@direccion", envio.Direccion);
                    cmd.Parameters.AddWithValue("@ciudad", envio.Ciudad);
                    cmd.Parameters.AddWithValue("@provincia", envio.Provincia);
                    cmd.Parameters.AddWithValue("@cd", envio.Cd);
                    cmd.Parameters.AddWithValue("@costo", envio.Costo);
                    cmd.Parameters.AddWithValue("@fecha_hora", envio.FechaHora);

                    cmd.CommandType = CommandType.Text;
                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine("Actualización de envío exitosa");
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
