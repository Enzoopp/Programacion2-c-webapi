using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;
using System.Collections.Generic;
using System.Linq;

namespace Biblioteca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonasController : ControllerBase
    {
        // Lista estática en memoria (simula la DB)
        private static List<Persona> _listaPersonas = new List<Persona>
        {
            new Persona { Id = 1, Nombre = "Enzo", Apellido = "Pitana", FechaNacimiento = new DateTime(1998, 5, 20), Dni = "46976644" },
            new Persona { Id = 2, Nombre = "Lucía", Apellido = "Fernández", FechaNacimiento = new DateTime(2000, 3, 15), Dni = "40234567" },
            new Persona { Id = 3, Nombre = "Martín", Apellido = "Gómez", FechaNacimiento = new DateTime(1995, 7, 10), Dni = "38911223" },
            new Persona { Id = 4, Nombre = "Carla", Apellido = "López", FechaNacimiento = new DateTime(1992, 11, 2), Dni = "37655433" },
            new Persona { Id = 5, Nombre = "Santiago", Apellido = "Rodríguez", FechaNacimiento = new DateTime(1989, 1, 30), Dni = "35466778" }
        };

        // GET: api/personas
        [HttpGet]
        public ActionResult<List<Persona>> GetAll()
        {
            return Ok(_listaPersonas);
        }

        // GET: api/personas/{id}
        [HttpGet("{id}")]
        public ActionResult<Persona> GetById(int id)
        {
            var persona = _listaPersonas.FirstOrDefault(p => p.Id == id);
            if (persona == null) return NotFound();

            return Ok(persona);
        }

        // POST: api/personas
        [HttpPost]
        public ActionResult<Persona> Create([FromBody] Persona persona)
        {
            int lastId = _listaPersonas.Max(p => p.Id);
            persona.Id = lastId + 1;

            _listaPersonas.Add(persona);

            return CreatedAtAction(nameof(GetById), new { id = persona.Id }, persona);
        }

       // PUT: api/personas/{id}
        [HttpPut("{id}")]
        public ActionResult<Persona> Update(int id, [FromBody] Persona personaActualizada)
{
            var persona = _listaPersonas.FirstOrDefault(p => p.Id == id);
            if (persona == null) 
                return NotFound();

            persona.Nombre = personaActualizada.Nombre;
            persona.Apellido = personaActualizada.Apellido;
            persona.FechaNacimiento = personaActualizada.FechaNacimiento;
            persona.Dni = personaActualizada.Dni;

    return Ok(persona); // <- devuelve la persona actualizada
}


        // DELETE: api/personas/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var persona = _listaPersonas.FirstOrDefault(p => p.Id == id);
            if (persona == null) return NotFound($"No se encontro la persona con id: {id}");

            _listaPersonas.Remove(persona);
            return NoContent();
        }
    }
}
