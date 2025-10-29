using SistemaDeConsultas.Models;
using SistemaDeConsultas.Repositories;
using System;

namespace SistemaDeConsultas.Services
{
    public static class AgendamentoService
    {
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