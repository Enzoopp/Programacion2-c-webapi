using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MiPrimeraApi.Models;

namespace MiPrimeraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/personas
    public class PersonasController : ControllerBase
    {
        private static readonly List<Persona> Personas = new()
        {
            new Persona { Id = 1, Nombre = "Juan", Apellido = "Pérez" },
            new Persona { Id = 2, Nombre = "Ana",  Apellido = "Gómez" },
            new Persona { Id = 3, Nombre = "Luis", Apellido = "Martínez" }
        };

        //GET /api/personas
        [HttpGet]
        public ActionResult<IEnumerable<Persona>> GetAll() => Ok(Personas);

        //GET /api/personas/{id}
        [HttpGet("{id:int}")]
        public ActionResult<Persona> GetById(int id)
        {
            var persona = Personas.FirstOrDefault(p => p.Id == id);
            if (persona is null) return NotFound();
            return Ok(persona);
        }

        //POST /api/personas
        [HttpPost]
        public ActionResult<Persona> Post([FromBody] Persona persona)
        {
            persona.Id = Personas.Count == 0 ? 1 : Personas.Max(p => p.Id) + 1;
            Personas.Add(persona);
            return CreatedAtAction(nameof(GetById), new { id = persona.Id }, persona);
        }

        //PUT /api/personas/{id}
        [HttpPut("{id:int}")]
        public IActionResult Put(int id, [FromBody] Persona persona)
        {
            var existing = Personas.FirstOrDefault(p => p.Id == id);
            if (existing is null) return NotFound();
            existing.Nombre = persona.Nombre;
            existing.Apellido = persona.Apellido;
            return NoContent();
        }

        //DELETE /api/personas/{id}
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var persona = Personas.FirstOrDefault(p => p.Id == id);
            if (persona is null) return NotFound();
            Personas.Remove(persona);
            return NoContent();
        }
    }
}