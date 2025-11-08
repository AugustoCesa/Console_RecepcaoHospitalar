using SistemaDeConsultas.Models;
using System.Collections.Generic;
using System.Linq;

namespace SistemaDeConsultas.Repositories
{
    public static class MedicoRepository
    {
        private static readonly List<Medico> _medicos = new List<Medico>();
        private static int _nextId = 1;

        public static Medico Adicionar(Medico medico)
        {
            medico.Id = _nextId++;
            _medicos.Add(medico);
            return medico;
        }

        public static Medico ObterPorId(int id)
        {
            return _medicos.FirstOrDefault(m => m.Id == id);
        }

        public static List<Medico> ListarTodos()
        {
            return _medicos;
        }

        // Buscas padronizadas
        public static List<Medico> BuscarPorNome(string nomeParcial)
        {
            if (string.IsNullOrWhiteSpace(nomeParcial)) return new List<Medico>();
            return _medicos.Where(m => m.Nome != null && m.Nome.ToLower().Contains(nomeParcial.Trim().ToLower())).ToList();
        }

        public static Medico ObterPorCRM(string crm)
        {
            if (string.IsNullOrWhiteSpace(crm)) return null;
            return _medicos.FirstOrDefault(m => m.CRM == crm);
        }

        // RF-011
        public static List<Medico> ListarPorEspecialidade(Especialidade especialidade)
        {
            return _medicos.Where(m => m.Especialidade == especialidade).ToList();
        }

        public static bool Remover(int id)
        {
            var m = ObterPorId(id);
            if (m != null)
            {
                return _medicos.Remove(m);
            }
            return false;
        }
    }
}