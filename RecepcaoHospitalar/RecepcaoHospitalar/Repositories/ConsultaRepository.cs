using SistemaDeConsultas.Models;
using System.Collections.Generic;
using System.Linq;

namespace SistemaDeConsultas.Repositories
{
    public static class ConsultaRepository
    {
        private static readonly List<Consulta> _consultas = new List<Consulta>();
        private static int _nextId = 1;

        public static Consulta Adicionar(Consulta consulta)
        {
            consulta.Id = _nextId++;
            _consultas.Add(consulta);
            return consulta;
        }

        public static List<Consulta> ListarTodas()
        {
            return _consultas;
        }

        public static List<Consulta> ListarPorMedico(int medicoId)
        {
            return _consultas.Where(c => c.MedicoId == medicoId).ToList();
        }

        public static Consulta ObterPorId(int id)
        {
            return _consultas.FirstOrDefault(c => c.Id == id);
        }

        public static void AtualizarStatus(int id, StatusConsulta novoStatus)
        {
            var consulta = ObterPorId(id);
            if (consulta != null)
            {
                consulta.Status = novoStatus;
            }
        }

        public static List<Consulta> ListarPorPeriodo(DateTime inicio, DateTime fim)
        {
            return _consultas.Where(c => c.DataHoraInicio >= inicio && c.DataHoraInicio <= fim).ToList();
        }

        public static List<Consulta> ListarPorStatus(StatusConsulta status)
        {
            return _consultas.Where(c => c.Status == status).ToList();
        }
    }
}