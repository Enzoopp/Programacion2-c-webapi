using Microsoft.AspNetCore.Mvc;
using MiPrimeraApi.Models; // <-- corregido el namespace

namespace MiPrimeraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReunionesController : ControllerBase
    {
        private static readonly List<Reunion> Reuniones = new List<Reunion>()
        {
            new Reunion { Id = 1, Titulo = "Reunión de equipo", Fecha = DateTime.Now, Lugar = "Sala 1" },
            new Reunion { Id = 2, Titulo = "Reunión de proyecto", Fecha = DateTime.Now.AddDays(1), Lugar = "Sala 2" }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Reunion>> Get()
        {
            return Ok(Reuniones);
        }

        [HttpGet("{id}")]
        public ActionResult<Reunion> Get(int id)
        {
            var reunion = Reuniones.FirstOrDefault(r => r.Id == id);
            if (reunion == null)
            {
                return NotFound();
            }
            return Ok(reunion);
        }

        [HttpPost]
        public ActionResult<Reunion> Post([FromBody] Reunion reunion)
        {
            if (reunion == null || string.IsNullOrWhiteSpace(reunion.Titulo) || string.IsNullOrWhiteSpace(reunion.Lugar))
            {
                return BadRequest("Título y Lugar son requeridos.");
            }
            reunion.Id = Reuniones.Count == 0 ? 1 : Reuniones.Max(r => r.Id) + 1;
            Reuniones.Add(reunion);
            return CreatedAtAction(nameof(Get), new { id = reunion.Id }, reunion);
        }

        [HttpPut("{id}")]
        public ActionResult<Reunion> Put(int id, [FromBody] Reunion reunion)
        {
            var existingReunion = Reuniones.FirstOrDefault(r => r.Id == id);
            if (existingReunion == null)
            {
                return NotFound();
            }
            if (reunion == null || string.IsNullOrWhiteSpace(reunion.Titulo) || string.IsNullOrWhiteSpace(reunion.Lugar))
            {
                return BadRequest("Título y Lugar son requeridos.");
            }
            existingReunion.Titulo = reunion.Titulo;
            existingReunion.Fecha = reunion.Fecha;
            existingReunion.Lugar = reunion.Lugar;
            existingReunion.Participantes = reunion.Participantes;
            return Ok(existingReunion);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var reunion = Reuniones.FirstOrDefault(r => r.Id == id);
            if (reunion == null)
            {
                return NotFound();
            }
            Reuniones.Remove(reunion);
            return NoContent();
        }
    }
}