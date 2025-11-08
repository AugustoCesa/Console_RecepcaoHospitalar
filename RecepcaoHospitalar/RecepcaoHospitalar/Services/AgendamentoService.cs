using SistemaDeConsultas.Models;
using SistemaDeConsultas.Repositories;
using System;

namespace SistemaDeConsultas.Services
{
    public static class AgendamentoService
    {
        private static readonly TimeSpan HorarioInicio = new TimeSpan(7, 0, 0); // 07:00
        private static readonly TimeSpan HorarioFim = new TimeSpan(17, 30, 0); // 17:30

        public static bool ValidarHorarioConsulta(DateTime horario)
        {
            if (horario == default(DateTime))
                return false;

            TimeSpan horaConsulta = horario.TimeOfDay;
            return horaConsulta >= HorarioInicio && horaConsulta <= HorarioFim;
        }

        // RF-015, RF-016: O sistema deve calcular automaticamente conflitos
        public static bool VerificarConflito(int medicoId, DateTime novoInicio, DateTime novoFim)
        {
            var consultasDoMedico = ConsultaRepository.ListarPorMedico(medicoId);

            foreach (var consultaExistente in consultasDoMedico)
            {
                // Ignora consultas canceladas
                if (consultaExistente.Status == StatusConsulta.Cancelada)
                    continue;

                // Lógica de sobreposição de tempo:
                // (StartA < EndB) e (EndA > StartB)
                bool haConflito = (novoInicio < consultaExistente.DataHoraFim) &&
                                  (novoFim > consultaExistente.DataHoraInicio);

                if (haConflito)
                {
                    return true; // Conflito encontrado
                }
            }
            return false; // Sem conflitos
        }
    }
}