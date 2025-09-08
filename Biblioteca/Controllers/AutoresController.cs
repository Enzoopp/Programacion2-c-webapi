using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;
using System.Collections.Generic;
using System.Linq;
using Biblioteca.interfaces;

namespace Biblioteca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutorController : ControllerBase
    {
        private readonly IAutorService _autorService;
        public AutorController(IAutorService autorService)
        {
            _autorService = autorService;
        }

        // GET: api/autores
        [HttpGet]
        public ActionResult<List<Autor>> GetAll()
        {
            return Ok(_autorService.GetAll());
        }

        // GET: api/autores/{id}
        [HttpGet("{id}")]
        public ActionResult<Autor> GetById(int id)
        {
            var a = _autorService.GetById(id);
            if (a != null)
            {
                return Ok(a);
            }
            else
            {
                return NotFound($"No se encontro el autor con id: {id}");
            }
        }

        // POST: api/autores
        [HttpPost]
        public ActionResult<Autor> Create([FromBody] Autor autor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newAutor = _autorService.Create(autor); 

            return CreatedAtAction(nameof(GetById), new { id = newAutor.Id }, newAutor);
        }

       // PUT: api/autores/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Autor autor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAutor = _autorService.GetById(id);
            if (existingAutor == null)
            {
                return NotFound($"No se encontro el autor con id: {id}");
            }

            _autorService.Update(id, autor);
            return NoContent();
        }

        // DELETE: api/autores/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existingAutor = _autorService.GetById(id);
            if (existingAutor == null)
            {
                return NotFound($"No se encontro el autor con id: {id}");
            }

            _autorService.Delete(id);
            return NoContent();
        }
    }
}