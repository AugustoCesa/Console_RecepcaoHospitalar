using System;

namespace SistemaDeConsultas.Models
{
    public class Consulta
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int MedicoId { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public int DuracaoMinutos { get; set; }

        public DateTime DataHoraFim => DataHoraInicio.AddMinutes(DuracaoMinutos);

        public StatusConsulta Status { get; set; }
        public string Motivo { get; set; }
    }
}