using SistemaDeConsultas.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaDeConsultas.Repositories
{
    public static class PacienteRepository
    {
        private static readonly List<Paciente> _pacientes = new List<Paciente>();
        private static int _nextId = 1;

        // RF-003, RF-004, RF-005, RF-008
        public static Paciente Adicionar(Paciente paciente)
        {
            if (ObterPorCpf(paciente.CPF) != null)
            {
                throw new InvalidOperationException("Já existe um paciente com este CPF.");
            }

            paciente.Id = _nextId++;
            paciente.Status = StatusPaciente.Ativo;
            _pacientes.Add(paciente);
            return paciente;
        }

        public static Paciente ObterPorCpf(string cpf)
        {
            return _pacientes.FirstOrDefault(p => p.CPF == cpf);
        }

        public static Paciente ObterPorId(int id)
        {
            return _pacientes.FirstOrDefault(p => p.Id == id);
        }

        public static void AtualizarStatus(int id, StatusPaciente status)
        {
            var p = ObterPorId(id);
            if (p != null)
            {
                p.Status = status;
            }
        }

        public static bool Remover(int id)
        {
            var p = ObterPorId(id);
            if (p != null)
            {
                return _pacientes.Remove(p);
            }
            return false;
        }

        public static List<Paciente> ListarTodos()
        {
            return _pacientes;
        }

        // Buscas padronizadas
        public static List<Paciente> BuscarPorNome(string nomeParcial)
        {
            if (string.IsNullOrWhiteSpace(nomeParcial)) return new List<Paciente>();
            return _pacientes.Where(p => p.Nome != null && p.Nome.ToLower().Contains(nomeParcial.Trim().ToLower())).ToList();
        }

        public static List<Paciente> BuscarPorTelefone(string telefoneParcial)
        {
            if (string.IsNullOrWhiteSpace(telefoneParcial)) return new List<Paciente>();
            return _pacientes.Where(p => p.Telefone != null && p.Telefone.ToLower().Contains(telefoneParcial.Trim().ToLower())).ToList();
        }

        public static List<Paciente> BuscarPorStatus(StatusPaciente status)
        {
            return _pacientes.Where(p => p.Status == status).ToList();
        }

        public static List<Paciente> BuscarPorEspecialidade(Especialidade especialidade)
        {
            return _pacientes.Where(p => p.EspecialidadePreferida == especialidade).ToList();
        }

        // Atualiza automaticamente o status dos pacientes com base em regras simples:
        // - Se tiver 2 ou mais ausências (StatusConsulta.Ausente) nos últimos 365 dias = Suspenso
        // - Se não tiver consultas (agendadas ou concluídas) nos últimos 365 dias = Inativo
        // - Caso contrário = Ativo
        public static void AtualizarStatusAutomatico()
        {
            var todas = ConsultaRepository.ListarTodas();
            var limite = DateTime.Now.AddDays(-365);

            foreach (var p in _pacientes)
            {
                var consultasPaciente = todas.Where(c => c.PacienteId == p.Id).ToList();

                int ausenciasRecentes = consultasPaciente.Count(c => c.Status == StatusConsulta.Ausente && c.DataHoraInicio >= limite);
                if (ausenciasRecentes >= 2)
                {
                    p.Status = StatusPaciente.Suspenso;
                    continue;
                }

                bool temAtividadeRecente = consultasPaciente.Any(c => (c.Status == StatusConsulta.Agendada || c.Status == StatusConsulta.Concluida) && c.DataHoraInicio >= limite);
                if (!temAtividadeRecente)
                {
                    p.Status = StatusPaciente.Inativo;
                    continue;
                }

                p.Status = StatusPaciente.Ativo;
            }
        }
    }
}