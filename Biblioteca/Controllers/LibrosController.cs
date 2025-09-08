using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;
using System.Collections.Generic;
using System.Linq;
using Biblioteca.interfaces;

namespace Biblioteca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibroController : ControllerBase
    {
        private readonly ILibroService _libroService;
        public LibroController(ILibroService libroService)
        {
            _libroService = libroService;
        }

        // GET: api/libros
        [HttpGet]
        public ActionResult<List<Libro>> GetAll()
        {
            return Ok(_libroService.GetAll());
        }

        // GET: api/libros/{id}
        [HttpGet("{id}")]
        public ActionResult<Libro> GetById(int id)
        {
            var l = _libroService.GetById(id);
            if (l != null)
            {
                return Ok(l);
            }
            else
            {
                return NotFound($"No se encontro el libro con id: {id}");
            }
        }

        // POST: api/libros
        [HttpPost]
        public ActionResult<Libro> Create([FromBody] Libro libro)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newLibro = _libroService.Create(libro); 

            return CreatedAtAction(nameof(GetById), new { id = newLibro.Id }, newLibro);
        }

       // PUT: api/libros/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Libro libro)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingLibro = _libroService.GetById(id);
            if (existingLibro == null)
            {
                return NotFound($"No se encontro el libro con id: {id}");
            }

            _libroService.Update(id, libro);
            return NoContent();
        }

        // DELETE: api/libros/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existingLibro = _libroService.GetById(id);
            if (existingLibro == null)
            {
                return NotFound($"No se encontro el libro con id: {id}");
            }

            _libroService.Delete(id);
            return NoContent();
        }
    }
}