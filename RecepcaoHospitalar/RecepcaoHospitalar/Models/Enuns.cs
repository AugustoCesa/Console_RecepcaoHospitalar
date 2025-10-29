namespace SistemaDeConsultas.Models
{
    public enum Especialidade
    {
        ClinicoGeral,
        Cardiologia,
        Dermatologia,
        Ortopedia
    }

    public enum StatusPaciente
    {
        Ativo,
        Inativo,
        Suspenso
    }

    public enum StatusConsulta
    {
        Agendada,
        Concluida,
        Cancelada,
        Ausente // No-show
    }
}