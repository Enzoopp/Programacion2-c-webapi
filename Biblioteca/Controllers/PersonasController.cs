using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;
using System.Collections.Generic;
using System.Linq;
using Biblioteca.interfaces;

namespace Biblioteca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonasController : ControllerBase
    {
        private readonly IPersonaService _personaService;
        public PersonasController(IPersonaService personaService)
        {
            _personaService = personaService;
        }



        // GET: api/personas
        [HttpGet]
        public ActionResult<List<Persona>> GetAll()
        {
            return Ok(_personaService.GetAll());
        }

        // GET: api/personas/{id}
        [HttpGet("{id}")]
        public ActionResult<Persona> GetById(int id)
        {
            var p = _personaService.GetById(id);
            if (p != null)
            {
                return Ok(p);
            }
            else
            {
                return NotFound($"No se encontro la persona con id: {id}");
            }
        }

        // POST: api/personas
        [HttpPost]
        public ActionResult<Persona> Create([FromBody] Persona persona)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newPersona = _personaService.Create(persona); 

            return CreatedAtAction(nameof(GetById), new { id = newPersona.Id }, newPersona);
        }

       // PUT: api/personas/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Persona persona)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var personaExistente = _personaService.GetById(id);
            if (personaExistente == null)
            {
                return NotFound($"No se encontro la persona con id: {id}");
            }

            _personaService.Update(id, persona);
            return NoContent();
        }


        // DELETE: api/personas/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var persona = _personaService.GetById(id);
            if (persona == null) return NotFound($"No se encontro la persona con id: {id}");

            _personaService.Delete(id);
            return NoContent();
        }
    }
}
