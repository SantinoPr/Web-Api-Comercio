using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private readonly VentaData _data;

        public VentaController(VentaData data)
        {
            _data = data;
        }
        // GET: api/<VentaController>
        [HttpGet]
        public async Task<IEnumerable<Venta>> Get()
        {
            IEnumerable<Venta> ventas = await _data.GetAllVentas();
            return ventas;
        }

        // GET api/<VentaController>/5
        [HttpGet("{id}")]
        public async Task<Venta> Get(int id)
        {
            Venta venta = await _data.GetVentaById(id);
            return venta;
        }

        // POST api/<VentaController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<VentaController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<VentaController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
