namespace MiPrimeraApi.Models
{
    public class Reunion
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime Fecha { get; set; }
        public string Lugar { get; set; }
        public List<string>? Participantes { get; set; }
    }
}