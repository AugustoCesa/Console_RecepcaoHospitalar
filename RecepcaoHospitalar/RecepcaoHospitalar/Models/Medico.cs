namespace SistemaDeConsultas.Models
{
    public class Medico
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CRM { get; set; }
        public Especialidade Especialidade { get; set; }
    }
}