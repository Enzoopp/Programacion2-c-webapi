using Microsoft.AspNetCore.Mvc;
using MiPrimeraApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace MiPrimeraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnosController : ControllerBase
    {
        private static readonly List<Turno> Turnos = new List<Turno>()
        {
            new Turno { Id = 1, Fecha = DateTime.Now, HoraInicio = "09:00", HoraFin = "10:00", Estado = "reservado", EmpleadoId = 1 },
            new Turno { Id = 2, Fecha = DateTime.Now.AddDays(1), HoraInicio = "10:00", HoraFin = "11:00", Estado = "completado", EmpleadoId = 2 }
        };

        //GET /api/turnos?estado=reservado
        //obtiene una lista de todos los turnos con filtro opcional por estado
        [HttpGet]
        public ActionResult<IEnumerable<Turno>> Get([FromQuery] string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return Ok(Turnos);
            }
            var turnosFiltrados = Turnos.Where(t => t.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(turnosFiltrados);
        }

        //GET /api/turnos/{id}
        //obtiene los detalles de un turno específico por su ID
        [HttpGet("{id}")]
        public ActionResult<Turno> Get(int id)
        {
            var turno = Turnos.FirstOrDefault(t => t.Id == id);
            if (turno == null)
            {
                return NotFound();
            }
            return Ok(turno);
        }

        //POST /api/turnos
        //crea un nuevo turno
        [HttpPost]
        public ActionResult<Turno> Post([FromBody] Turno turno)
        {
            if (turno == null || string.IsNullOrWhiteSpace(turno.HoraInicio) || string.IsNullOrWhiteSpace(turno.HoraFin))
            {
                return BadRequest("Hora de Inicio y Hora de Fin son requeridos");
            }
            turno.Id = Turnos.Count == 0 ? 1 : Turnos.Max(t => t.Id) + 1;
            Turnos.Add(turno);
            return CreatedAtAction(nameof(Get), new { id = turno.Id }, turno);
        }

        //PUT /api/turnos/{id}
        //actualiza la informacion de un turno especifico
        [HttpPut("{id}")]
        public ActionResult<Turno> Put(int id, [FromBody] Turno turno)
        {
            var existingTurno = Turnos.FirstOrDefault(t => t.Id == id);
            if (existingTurno == null)
            {
                return NotFound();
            }
            if (turno == null || string.IsNullOrWhiteSpace(turno.HoraInicio) || string.IsNullOrWhiteSpace(turno.HoraFin))
            {
                return BadRequest("Hora de Inicio y Hora de Fin son requeridos.");
            }
            existingTurno.Fecha = turno.Fecha;
            existingTurno.HoraInicio = turno.HoraInicio;
            existingTurno.HoraFin = turno.HoraFin;
            existingTurno.Estado = turno.Estado;
            existingTurno.EmpleadoId = turno.EmpleadoId;
            return Ok(existingTurno);
        }

        //DELETE /api/turnos/{id}
        //elimina un turno
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var turno = Turnos.FirstOrDefault(t => t.Id == id);
            if (turno == null)
            {
                return NotFound();
            }
            Turnos.Remove(turno);
            return NoContent();
        }

        //POST /api/turnos/{id}/confirmacion
        //confirma un turno y envia una notificacion
        [HttpPost("{id}/confirmacion")]
        public IActionResult ConfirmarTurno(int id)
        {
            var turno = Turnos.FirstOrDefault(t => t.Id == id);
            if (turno == null)
            {
                return NotFound("Turno no encontrado.");
            }

            //logica para confirmar el turno y enviar la notificación
            turno.Estado = "confirmado";
            
            return Ok($"Turno {id} confirmado y notificación enviada.");
        }
    }
}