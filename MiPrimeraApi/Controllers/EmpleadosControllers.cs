using Microsoft.AspNetCore.Mvc;
using MiPrimeraApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace MiPrimeraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadosController : ControllerBase
    {
        private static readonly List<Empleado> Empleados = new List<Empleado>()
        {
            new Empleado { Id = 1, Nombre = "Pedro", Rol = "medico" },
            new Empleado { Id = 2, Nombre = "María", Rol = "enfermera" },
            new Empleado { Id = 3, Nombre = "Carlos", Rol = "medico" }
        };

        //GET /api/empleados?rol=medico
        //obtiene una lista de todos los empleados con filtro opcional por rol
        [HttpGet]
        public ActionResult<IEnumerable<Empleado>> Get([FromQuery] string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
            {
                return Ok(Empleados);
            }
            var empleadosFiltrados = Empleados.Where(e => e.Rol.Equals(rol, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(empleadosFiltrados);
        }

        //GET /api/empleados/{id}
        //obtiene los detalles de un empleado específico por su ID
        [HttpGet("{id}")]
        public ActionResult<Empleado> Get(int id)
        {
            var empleado = Empleados.FirstOrDefault(e => e.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }
            return Ok(empleado);
        }

        //POST /api/empleados
        //crea un nuevo empleado
        [HttpPost]
        public ActionResult<Empleado> Post([FromBody] Empleado empleado)
        {
            if (empleado == null || string.IsNullOrWhiteSpace(empleado.Nombre))
            {
                return BadRequest("El nombre del empleado es requerido.");
            }
            empleado.Id = Empleados.Count == 0 ? 1 : Empleados.Max(e => e.Id) + 1;
            Empleados.Add(empleado);
            return CreatedAtAction(nameof(Get), new { id = empleado.Id }, empleado);
        }

        //PUT /api/empleados/{id}
        //actualiza la informacion de un empleado especifico
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Empleado empleado)
        {
            var existingEmpleado = Empleados.FirstOrDefault(e => e.Id == id);
            if (existingEmpleado == null)
            {
                return NotFound();
            }
            if (empleado == null || string.IsNullOrWhiteSpace(empleado.Nombre))
            {
                return BadRequest("El nombre del empleado es requerido.");
            }
            existingEmpleado.Nombre = empleado.Nombre;
            existingEmpleado.Rol = empleado.Rol;
            return NoContent();
        }

        //DELETE /api/empleados/{id}
        //elimina un empleado
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var empleado = Empleados.FirstOrDefault(e => e.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }
            Empleados.Remove(empleado);
            return NoContent();
        }

        //GET /api/empleados/{id}/disponibilidad
        //obtiene los horarios disponibles de un empleado
        [HttpGet("{id}/disponibilidad")]
        public ActionResult<IEnumerable<string>> GetDisponibilidad(int id)
        {
            var empleado = Empleados.FirstOrDefault(e => e.Id == id);
            if (empleado == null)
            {
                return NotFound("Empleado no encontrado.");
            }

            //logica para simular la disponibilidad de un empleado
            var disponibilidad = new List<string>
            {
                "Lunes: 09:00 - 12:00",
                "Martes: 14:00 - 18:00",
                "Miércoles: 09:00 - 12:00"
            };
            
            return Ok(disponibilidad);
        }
    }
}