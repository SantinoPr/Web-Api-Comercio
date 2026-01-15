using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProductoController : ControllerBase
    {
        private readonly ProductoData _data;

        public ProductoController(ProductoData data)
        {
            _data = data;
        }   
        // GET: api/<ProductoController>
        [HttpGet]
        public async Task<IEnumerable<Producto>> Get()
        {
            IEnumerable<Producto> productos = await _data.GetAllProductos();
            return productos;
        }

        // GET api/<ProductoController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ProductoController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ProductoController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProductoController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
