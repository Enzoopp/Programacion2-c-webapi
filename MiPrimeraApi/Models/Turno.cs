using System;

namespace MiPrimeraApi.Models
{
    public class Turno
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }        // fecha del turno
        public string HoraInicio { get; set; }     // hora de inicio
        public string HoraFin { get; set; }        // hora de fin
        public string Estado { get; set; }         // estado del turno
        public int EmpleadoId { get; set; }        // relación con empleado
    }
}
