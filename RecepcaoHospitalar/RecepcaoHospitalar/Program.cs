using System;
using System.Linq;
using SistemaDeConsultas.Models;
using SistemaDeConsultas.Repositories;
using SistemaDeConsultas.Services;

namespace SistemaDeConsultas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            PrintHeader("Sistema de Recepção Hospitalar");
            InicializarDados(); // Pré-cadastra dados para teste

            bool executando = true;
            while (executando)
            {
                MostrarMenuPrincipal();
                string opcao = Console.ReadLine() ?? "";

                switch (opcao)
                {
                    case "1":
                        GerenciarPacientes();
                        break;
                    case "2":
                        GerenciarMedicos();
                        break;
                    case "3":
                        AgendarConsulta();
                        break;
                    case "4":
                        GerenciarConsultas();
                        break;
                    case "5":
                        GerarRelatorios();
                        break;
                    case "0":
                        executando = false;
                        break;
                    default:
                        PrintError("Opção inválida. Tente novamente.");
                        break;
                }
                Pausar();
            }
            Console.WriteLine("Obrigado por usar o sistema!");
        }

        private static void MostrarMenuPrincipal()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("========================================");
            Console.WriteLine("          Menu Principal");
            Console.WriteLine("========================================\n");
            Console.ResetColor();
            Console.WriteLine(" 1. Gerenciar Pacientes   2. Gerenciar Médicos");
            Console.WriteLine(" 3. Agendar Consulta      4. Gerenciar Consultas");
            Console.WriteLine(" 5. Relatórios                                 ");
            Console.WriteLine(" 0. Sair");
            Console.Write("Escolha uma opção: ");
        }

        // RF-001 a RF-008
        private static void CadastrarPaciente()
        {
            try
            {
                PrintHeader("Cadastro de Paciente");
                string nome = LerStringNaoVazia("Nome: ");
                string cpf = LerStringNaoVazia("CPF (apenas números): ");
                string telefone = LerStringNaoVazia("Telefone: ");
                Especialidade esp = SelecionarEspecialidade("Especialidade Preferida (RF-007):");

                Paciente novoPaciente = new Paciente
                {
                    Nome = nome,
                    CPF = cpf,
                    Telefone = telefone,
                    EspecialidadePreferida = esp
                };

                PacienteRepository.Adicionar(novoPaciente); // RF-005 (validação de CPF) é feito no repositório
                Console.WriteLine($"\nSUCESSO: Paciente '{nome}' (ID: {novoPaciente.Id}) cadastrado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERRO: {ex.Message}");
            }
        }

        // RF-002
        private static void CadastrarMedico()
        {
            try
            {
                PrintHeader("Cadastro de Médico");
                string nome = LerStringNaoVazia("Nome: ");
                string crm = LerStringNaoVazia("CRM: ");
                Especialidade esp = SelecionarEspecialidade("Especialidade do Médico:");

                Medico novoMedico = new Medico
                {
                    Nome = nome,
                    CRM = crm,
                    Especialidade = esp
                };

                MedicoRepository.Adicionar(novoMedico);
                Console.WriteLine($"\nSUCESSO: Médico '{nome}' (ID: {novoMedico.Id}) cadastrado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERRO: {ex.Message}");
            }
        }

        // RF-009 a RF-018
        private static void AgendarConsulta()
        {
            try
            {
                PrintHeader("Agendamento de Consulta");

                // 1. Selecionar Paciente
                Console.WriteLine("\nComo deseja buscar o paciente?");
                Console.WriteLine("1 - Buscar por ID");
                Console.WriteLine("2 - Buscar por CPF");
                int opcaoBusca = LerInteiro("Digite a opção desejada: ");

                Paciente? paciente = null;
                if (opcaoBusca == 1)
                {
                    ListarPacientes();
                    int pacienteId = LerInteiro("Digite o ID do Paciente: ");
                    paciente = PacienteRepository.ObterPorId(pacienteId);
                }
                else if (opcaoBusca == 2)
                {
                    string cpf = LerStringNaoVazia("Digite o CPF do Paciente: ").Trim();
                    paciente = PacienteRepository.ObterPorCpf(cpf);
                }
                else
                {
                    Console.WriteLine("ERRO: Opção inválida.");
                    return;
                }

                if (paciente == null)
                {
                    Console.WriteLine("ERRO: Paciente não encontrado.");
                    return;
                }

                // 2. Selecionar Especialidade (RF-011)
                Especialidade espDesejada = SelecionarEspecialidade("Qual especialidade você busca?");

                // 3 Listar e Selecionar Médico (RF-011)
                var medicosDisponiveis = MedicoRepository.ListarPorEspecialidade(espDesejada);
                if (medicosDisponiveis.Count == 0)
                {
                    Console.WriteLine($"ERRO: Nenhum médico encontrado para a especialidade {espDesejada}.");
                    return;
                }

                Console.WriteLine($"\nMédicos encontrados para {espDesejada}:");
                foreach (var m in medicosDisponiveis)
                {
                    Console.WriteLine($"  ID: {m.Id} | Nome: {m.Nome} | CRM: {m.CRM}");
                }

                int medicoId = LerInteiro("Digite o ID do Médico: ");
                Medico? medico = medicosDisponiveis.FirstOrDefault(m => m.Id == medicoId);
                if (medico == null)
                {
                    Console.WriteLine("ERRO: ID do Médico inválido ou não pertence a esta especialidade.");
                    return;
                }

                // 4. Data e Hora
                DateTime dataHoraInicio = LerDataHora("Data e Hora (ex: 20/12/2025 14:30): ");

                // RF-013: Data não pode ser no passado
                if (dataHoraInicio < DateTime.Now)
                {
                    Console.WriteLine("ERRO: A data do agendamento não pode ser no passado.");
                    return;
                }

                // Validar horário de funcionamento (07:00 às 17:30)
                if (!AgendamentoService.ValidarHorarioConsulta(dataHoraInicio))
                {
                    Console.WriteLine("ERRO: O horário deve estar entre 07:00 e 17:30.");
                    return;
                }

                // 5. Duração
                int duracao = LerInteiro("Duração em minutos (ex: 30): ");

                if (duracao <= 0)
                {
                    Console.WriteLine("ERRO: A duração deve ser maior que zero.");
                    return;
                }

                DateTime dataHoraFim = dataHoraInicio.AddMinutes(duracao);
                string motivo = LerStringNaoVazia("Motivo da consulta: ");

                // 6. Verificar Conflitos
                bool haConflito = AgendamentoService.VerificarConflito(medicoId, dataHoraInicio, dataHoraFim);
                if (haConflito)
                {
                    Console.WriteLine($"ERRO: Conflito de agenda. O Dr(a). {medico.Nome} já possui um compromisso neste horário.");
                    return;
                }

                // 7. Criar a consulta
                Consulta novaConsulta = new Consulta
                {
                    PacienteId = paciente.Id,
                    MedicoId = medicoId,
                    DataHoraInicio = dataHoraInicio,
                    DuracaoMinutos = duracao,
                    Motivo = motivo,
                    Status = StatusConsulta.Agendada
                };

                ConsultaRepository.Adicionar(novaConsulta);
                // Atualiza status automático após novo agendamento (por exemplo, marcar paciente como ativo)
                PacienteRepository.AtualizarStatusAutomatico();

                PrintSuccess($"Consulta agendada (ID: {novaConsulta.Id})");
                PrintInfo($"  Paciente: {paciente.Nome}");
                PrintInfo($"  Médico: {medico.Nome} ({medico.Especialidade})");
                PrintInfo($"  Horário: {novaConsulta.DataHoraInicio:g} até {novaConsulta.DataHoraFim:t}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERRO inesperado: {ex.Message}");
            }
        }

        private static void ListarPacientes()
        {
            PrintHeader("Lista de Pacientes");
            // Atualiza status automaticamente antes de listar
            PacienteRepository.AtualizarStatusAutomatico();
            var pacientes = PacienteRepository.ListarTodos();
            if (pacientes.Count == 0)
            {
                PrintInfo("Nenhum paciente cadastrado.");
                return;
            }

            foreach (var p in pacientes)
            {
                Console.WriteLine($"ID: {p.Id} | Nome: {p.Nome} | CPF: {p.CPF} | Status: {p.Status}");
            }
        }

        private static void ListarMedicos()
        {
            PrintHeader("Lista de Médicos");
            var medicos = MedicoRepository.ListarTodos();
            if (medicos.Count == 0)
            {
                PrintInfo("Nenhum médico cadastrado.");
                return;
            }

            foreach (var m in medicos)
            {
                Console.WriteLine($"ID: {m.Id} | Nome: {m.Nome} | CRM: {m.CRM} | Especialidade: {m.Especialidade}");
            }
        }

        private static void BuscarPaciente()
        {
            PrintHeader("Buscar Paciente");
            Console.WriteLine("1. Por ID");
            Console.WriteLine("2. Por Nome");
            Console.WriteLine("3. Por CPF");
            Console.WriteLine("4. Por Telefone");
            Console.WriteLine("5. Por Status");
            Console.WriteLine("6. Por Especialidade Preferida");
            Console.WriteLine("0. Voltar");

            string opc = Console.ReadLine() ?? "";
            switch (opc)
            {
                case "1":
                    int id = LerInteiro("Digite o ID: ");
                    var p = PacienteRepository.ObterPorId(id);
                    if (p == null) PrintInfo("Paciente não encontrado."); else Console.WriteLine($"ID: {p.Id} | Nome: {p.Nome} | CPF: {p.CPF} | Tel: {p.Telefone} | Status: {p.Status}");
                    break;
                case "2":
                    string nome = LerStringNaoVazia("Nome (ou parte do nome): ");
                    var porNome = PacienteRepository.BuscarPorNome(nome);
                    ExibirListaPacientes(porNome);
                    break;
                case "3":
                    string cpf = LerStringNaoVazia("CPF: ");
                    var porCpf = PacienteRepository.ObterPorCpf(cpf);
                    if (porCpf == null) PrintInfo("Paciente não encontrado."); else Console.WriteLine($"ID: {porCpf.Id} | Nome: {porCpf.Nome} | CPF: {porCpf.CPF} | Tel: {porCpf.Telefone} | Status: {porCpf.Status}");
                    break;
                case "4":
                    string tel = LerStringNaoVazia("Telefone (ou parte): ");
                    var porTel = PacienteRepository.BuscarPorTelefone(tel);
                    ExibirListaPacientes(porTel);
                    break;
                case "5":
                    var vals = Enum.GetValues<StatusPaciente>();
                    for (int i = 0; i < vals.Length; i++) Console.WriteLine($"{i} - {vals[i]}");
                    int idx = LerInteiro("Escolha o número do status: ");
                    if (idx >= 0 && idx < vals.Length)
                    {
                        var porStatus = PacienteRepository.BuscarPorStatus(vals[idx]);
                        ExibirListaPacientes(porStatus);
                    }
                    else PrintError("Status inválido.");
                    break;
                case "6":
                    var esp = SelecionarEspecialidade("Escolha a especialidade preferida:");
                    var porEsp = PacienteRepository.BuscarPorEspecialidade(esp);
                    ExibirListaPacientes(porEsp);
                    break;
                case "0":
                    return;
                default:
                    PrintError("Opção inválida.");
                    break;
            }
        }

        private static void BuscarMedico()
        {
            PrintHeader("Buscar Médico");
            Console.WriteLine("1. Por ID");
            Console.WriteLine("2. Por Nome");
            Console.WriteLine("3. Por CRM");
            Console.WriteLine("4. Por Especialidade");
            Console.WriteLine("0. Voltar");

            string opc = Console.ReadLine() ?? "";
            switch (opc)
            {
                case "1":
                    int id = LerInteiro("Digite o ID: ");
                    var m = MedicoRepository.ObterPorId(id);
                    if (m == null) PrintInfo("Médico não encontrado."); else Console.WriteLine($"ID: {m.Id} | Nome: {m.Nome} | CRM: {m.CRM} | Especialidade: {m.Especialidade}");
                    break;
                case "2":
                    string nome = LerStringNaoVazia("Nome (ou parte do nome): ");
                    var porNome = MedicoRepository.BuscarPorNome(nome);
                    if (porNome.Count == 0) PrintInfo("Nenhum médico encontrado."); else foreach (var mm in porNome) Console.WriteLine($"ID: {mm.Id} | Nome: {mm.Nome} | CRM: {mm.CRM} | Esp: {mm.Especialidade}");
                    break;
                case "3":
                    string crm = LerStringNaoVazia("CRM: ");
                    var porCrm = MedicoRepository.ObterPorCRM(crm);
                    if (porCrm == null) PrintInfo("Médico não encontrado."); else Console.WriteLine($"ID: {porCrm.Id} | Nome: {porCrm.Nome} | CRM: {porCrm.CRM} | Esp: {porCrm.Especialidade}");
                    break;
                case "4":
                    var esp = SelecionarEspecialidade("Especialidade:");
                    var porEsp = MedicoRepository.ListarPorEspecialidade(esp);
                    if (porEsp.Count == 0) PrintInfo("Nenhum médico encontrado para essa especialidade."); else foreach (var mm in porEsp) Console.WriteLine($"ID: {mm.Id} | Nome: {mm.Nome} | CRM: {mm.CRM} | Esp: {mm.Especialidade}");
                    break;
                case "0":
                    return;
                default:
                    PrintError("Opção inválida.");
                    break;
            }
        }

        private static void ExibirListaPacientes(System.Collections.Generic.List<Paciente> pacientes)
        {
            if (pacientes == null || pacientes.Count == 0)
            {
                PrintInfo("Nenhum paciente encontrado.");
                return;
            }

            foreach (var p in pacientes)
            {
                Console.WriteLine($"ID: {p.Id} | Nome: {p.Nome} | CPF: {p.CPF} | Tel: {p.Telefone} | Status: {p.Status}");
            }
        }

        private static void ListarConsultas()
        {
            PrintHeader("Lista de Consultas Agendadas");
            var consultas = ConsultaRepository.ListarTodas();
            ExibirConsultas(consultas);
        }

        private static void ExibirConsultas(List<Consulta> consultas)
        {
            if (consultas.Count == 0)
            {
                PrintInfo("Nenhuma consulta encontrada.");
                return;
            }

            foreach (var c in consultas)
            {
                Paciente p = PacienteRepository.ObterPorId(c.PacienteId);
                Medico m = MedicoRepository.ObterPorId(c.MedicoId);

                Console.WriteLine($"ID: {c.Id} | Status: {c.Status}");
                Console.WriteLine($"  Data: {c.DataHoraInicio:dd/MM/yyyy}");
                Console.WriteLine($"  Hora: {c.DataHoraInicio:HH:mm} - {c.DataHoraFim:HH:mm} ({c.DuracaoMinutos} min)");
                Console.WriteLine($"  Paciente: {p?.Nome ?? "PACIENTE REMOVIDO"}");
                Console.WriteLine($"  Médico: {m?.Nome ?? "MÉDICO REMOVIDO"} ({m?.Especialidade})");
                Console.WriteLine($"  Motivo: {c.Motivo}");
                Console.WriteLine(new string('-', 20));
            }
        }

        private static void GerenciarConsultas()
        {
            while (true)
            {
                PrintHeader("Gerenciar Consultas");
                Console.WriteLine("1. Confirmar Presença");
                Console.WriteLine("2. Registrar Ausência");
                Console.WriteLine("3. Cancelar Consulta");
                Console.WriteLine("4. Reagendar Consulta");
                Console.WriteLine("5. Verificar Agenda (por dia)");
                Console.WriteLine("0. Voltar");

                string opcao = Console.ReadLine() ?? "";

                switch (opcao)
                {
                    case "1":
                        AtualizarStatusConsulta(StatusConsulta.Concluida);
                        break;
                    case "2":
                        AtualizarStatusConsulta(StatusConsulta.Ausente);
                        break;
                    case "3":
                        AtualizarStatusConsulta(StatusConsulta.Cancelada);
                        break;
                    case "4":
                        ReagendarConsulta();
                        break;
                    case "5":
                        VerificarAgendaDia();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }

        private static void GerenciarPacientes()
        {
            while (true)
            {
                PrintHeader("Gerenciar Pacientes");
                Console.WriteLine("1. Cadastrar Paciente");
                Console.WriteLine("2. Listar Pacientes");
                Console.WriteLine("3. Alterar Status do Paciente");
                Console.WriteLine("4. Remover Paciente");
                Console.WriteLine("5. Buscar Paciente");
                Console.WriteLine("0. Voltar");

                string opcao = Console.ReadLine() ?? "";
                switch (opcao)
                {
                    case "1":
                        CadastrarPaciente();
                        break;
                    case "2":
                        ListarPacientes();
                        break;
                    case "3":
                        try
                        {
                            ListarPacientes();
                            int id = LerInteiro("Digite o ID do paciente: ");
                            Console.WriteLine("Selecione o novo status:");
                            var vals = Enum.GetValues<StatusPaciente>();
                            for (int i = 0; i < vals.Length; i++) Console.WriteLine($"{i} - {vals[i]}");
                            int idx = LerInteiro("Escolha o número do status: ");
                            if (idx >= 0 && idx < vals.Length)
                            {
                                PacienteRepository.AtualizarStatus(id, vals[idx]);
                                PrintSuccess("Status atualizado com sucesso.");
                            }
                            else PrintError("Status inválido.");
                        }
                        catch (Exception ex)
                        {
                            PrintError($"Erro: {ex.Message}");
                        }
                        break;
                    case "4":
                        try
                        {
                            ListarPacientes();
                            int idRem = LerInteiro("Digite o ID do paciente a remover: ");
                            bool ok = PacienteRepository.Remover(idRem);
                            if (ok) PrintSuccess("Paciente removido."); else PrintError("Paciente não encontrado.");
                        }
                        catch (Exception ex)
                        {
                            PrintError($"Erro: {ex.Message}");
                        }
                        break;
                    case "5":
                        BuscarPaciente();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }

        private static void GerenciarMedicos()
        {
            while (true)
            {
                PrintHeader("Gerenciar Médicos");
                Console.WriteLine("1. Cadastrar Médico");
                Console.WriteLine("2. Listar Médicos");
                Console.WriteLine("3. Remover Médico");
                Console.WriteLine("4. Buscar Médico");
                Console.WriteLine("0. Voltar");

                string opcao = Console.ReadLine() ?? "";
                switch (opcao)
                {
                    case "1":
                        CadastrarMedico();
                        break;
                    case "2":
                        ListarMedicos();
                        break;
                    case "3":
                        try
                        {
                            ListarMedicos();
                            int idRem = LerInteiro("Digite o ID do médico a remover: ");
                            bool ok = MedicoRepository.Remover(idRem);
                            if (ok) PrintSuccess("Médico removido."); else PrintError("Médico não encontrado.");
                        }
                        catch (Exception ex)
                        {
                            PrintError($"Erro: {ex.Message}");
                        }
                        break;
                    case "4":
                        BuscarMedico();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }

        private static void AtualizarStatusConsulta(StatusConsulta novoStatus)
        {
            try
            {
                ListarConsultas();
                int consultaId = LerInteiro("\nDigite o ID da consulta: ");
                
                var consulta = ConsultaRepository.ObterPorId(consultaId);
                if (consulta == null)
                {
                    Console.WriteLine("Consulta não encontrada.");
                    return;
                }

                if (consulta.Status != StatusConsulta.Agendada)
                {
                    Console.WriteLine($"Não é possível alterar o status. A consulta já está {consulta.Status}.");
                    return;
                }

                ConsultaRepository.AtualizarStatus(consultaId, novoStatus);
                // Atualiza status automático dos pacientes, pois pode impactar (ex: marcar ausente)
                PacienteRepository.AtualizarStatusAutomatico();
                PrintSuccess($"Status da consulta atualizado para: {novoStatus}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar status: {ex.Message}");
            }
        }

        private static void ReagendarConsulta()
        {
            try
            {
                ListarConsultas();
                int consultaId = LerInteiro("\nDigite o ID da consulta a ser reagendada: ");
                
                var consulta = ConsultaRepository.ObterPorId(consultaId);
                if (consulta == null)
                {
                    Console.WriteLine("Consulta não encontrada.");
                    return;
                }

                if (consulta.Status != StatusConsulta.Agendada)
                {
                    Console.WriteLine($"Não é possível reagendar. A consulta já está {consulta.Status}.");
                    return;
                }

                // Copiar dados da consulta antiga
                var novaConsulta = new Consulta
                {
                    PacienteId = consulta.PacienteId,
                    MedicoId = consulta.MedicoId,
                    Motivo = consulta.Motivo
                };

                // Obter nova data e hora
                DateTime dataHoraInicio = LerDataHora("Nova Data e Hora (ex: 20/12/2025 14:30): ");
                
                if (dataHoraInicio < DateTime.Now)
                {
                    Console.WriteLine("A data do agendamento não pode ser no passado.");
                    return;
                }

                // Validar horário de funcionamento (07:00 às 17:30)
                if (!AgendamentoService.ValidarHorarioConsulta(dataHoraInicio))
                {
                    Console.WriteLine("ERRO: O horário deve estar entre 07:00 e 17:30.");
                    return;
                }

                novaConsulta.DataHoraInicio = dataHoraInicio;
                novaConsulta.DuracaoMinutos = consulta.DuracaoMinutos;

                // Verificar conflitos
                if (AgendamentoService.VerificarConflito(novaConsulta.MedicoId, novaConsulta.DataHoraInicio, novaConsulta.DataHoraFim))
                {
                    Console.WriteLine("Conflito de horário detectado. Escolha outro horário.");
                    return;
                }

                // Cancelar consulta antiga e criar nova
                ConsultaRepository.AtualizarStatus(consultaId, StatusConsulta.Cancelada);
                ConsultaRepository.Adicionar(novaConsulta);

                Console.WriteLine("Consulta reagendada com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao reagendar: {ex.Message}");
            }
        }

        private static void VerificarAgendaDia()
        {
            try
            {
                PrintHeader("Verificar Agenda por Dia");
                DateTime data = LerDataHora("Data (ex: 20/12/2025): ");
                DateTime inicio = data.Date;
                DateTime fim = inicio.AddDays(1).AddTicks(-1);

                var consultas = ConsultaRepository.ListarPorPeriodo(inicio, fim);
                Console.WriteLine($"\nConsultas em {inicio:dd/MM/yyyy}:");
                ExibirConsultas(consultas);
            }
            catch (Exception ex)
            {
                PrintError($"Erro ao verificar agenda: {ex.Message}");
            }
        }

        private static void GerarRelatorios()
        {
            while (true)
            {
                PrintHeader("Relatórios");
                Console.WriteLine("1. Consultas por Período");
                Console.WriteLine("2. Consultas por Status");
                Console.WriteLine("3. Taxa de Ausência");
                Console.WriteLine("0. Voltar");

                string opcao = Console.ReadLine() ?? "";

                switch (opcao)
                {
                    case "1":
                        RelatorioConsultasPorPeriodo();
                        break;
                    case "2":
                        RelatorioConsultasPorStatus();
                        break;
                    case "3":
                        RelatorioTaxaAusencia();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }

        private static void RelatorioConsultasPorPeriodo()
        {
            try
            {
                DateTime dataInicio = LerDataHora("Data Inicial (ex: 01/10/2025): ");
                DateTime dataFim = LerDataHora("Data Final (ex: 31/10/2025): ");

                if (dataFim < dataInicio)
                {
                    Console.WriteLine("A data final deve ser maior que a data inicial.");
                    return;
                }

                var consultas = ConsultaRepository.ListarPorPeriodo(dataInicio, dataFim);
                Console.WriteLine($"\nConsultas no período de {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}:");
                ExibirConsultas(consultas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
        }

        private static void RelatorioConsultasPorStatus()
        {
            Console.WriteLine("\nSelecione o status:");
            var statusValues = Enum.GetValues<StatusConsulta>();
            for (int i = 0; i < statusValues.Length; i++)
            {
                Console.WriteLine($"{i} - {statusValues[i]}");
            }

            int statusIndex = LerInteiro("Digite o número do status: ");
            if (statusIndex >= 0 && statusIndex < statusValues.Length)
            {
                var status = statusValues[statusIndex];
                var consultas = ConsultaRepository.ListarPorStatus(status);
                Console.WriteLine($"\nConsultas com status {status}:");
                ExibirConsultas(consultas);
            }
            else
            {
                Console.WriteLine("Status inválido.");
            }
        }

        private static void RelatorioTaxaAusencia()
        {
            var todasConsultas = ConsultaRepository.ListarTodas();
            int totalConcluidas = todasConsultas.Count(c => c.Status == StatusConsulta.Concluida);
            int totalAusentes = todasConsultas.Count(c => c.Status == StatusConsulta.Ausente);
            int totalFinalizadas = totalConcluidas + totalAusentes;

            if (totalFinalizadas == 0)
            {
                PrintInfo("Não há consultas finalizadas para calcular a taxa de ausência.");
                return;
            }

            decimal taxaAusencia = (decimal)totalAusentes / totalFinalizadas * 100;

            PrintHeader("Taxa de Ausência");
            Console.WriteLine($"Total de consultas finalizadas: {totalFinalizadas}");
            Console.WriteLine($"Consultas concluídas: {totalConcluidas}");
            Console.WriteLine($"Ausências: {totalAusentes}");
            Console.WriteLine($"Taxa de ausência: {taxaAusencia:F2}%");
        }


        // --- MÉTODOS AUXILIARES --

        private static void InicializarDados()
        {
            // Pré-cadastra alguns médicos e pacientes para facilitar os testes
            MedicoRepository.Adicionar(new Medico { Nome = "Dr. House", CRM = "12345-SP", Especialidade = Especialidade.ClinicoGeral });
            MedicoRepository.Adicionar(new Medico { Nome = "Dra. Grey", CRM = "54321-RJ", Especialidade = Especialidade.Cardiologia });
            MedicoRepository.Adicionar(new Medico { Nome = "Dr. Shepherd", CRM = "98765-MG", Especialidade = Especialidade.Ortopedia });

            PacienteRepository.Adicionar(new Paciente { Nome = "João da Silva", CPF = "11122233344", Telefone = "9999-8888", EspecialidadePreferida = Especialidade.Cardiologia });
            PacienteRepository.Adicionar(new Paciente { Nome = "Maria Souza", CPF = "55566677788", Telefone = "7777-6666", EspecialidadePreferida = Especialidade.ClinicoGeral });
            
            Console.WriteLine("Dados iniciais (médicos e pacientes) carregados para teste.");
        }
//Decoração
        private static void PrintHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine($"   {title}");
            Console.WriteLine("========================================\n");
            Console.ResetColor();
        }

        private static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static Especialidade SelecionarEspecialidade(string prompt)
        {
            Console.WriteLine(prompt);
            // Lista as opções do Enum
            var nomesEspecialidades = Enum.GetNames(typeof(Especialidade));
            for (int i = 0; i < nomesEspecialidades.Length; i++)
            {
                Console.WriteLine($"  {i} - {nomesEspecialidades[i]}");
            }

            while (true)
            {
                int indice = LerInteiro("Digite o número da especialidade: ");
                if (indice >= 0 && indice < nomesEspecialidades.Length)
                {
                    return (Especialidade)indice;
                }
                Console.WriteLine("Número inválido. Tente novamente.");
            }
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPressione [Enter] para continuar...");
            _ = Console.ReadLine();
        }

        // Funções robustas para ler entrada do usuário
        private static string LerStringNaoVazia(string prompt)
        {
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(input))
                {
                    PrintError("ERRO: Este campo não pode ficar vazio.");
                }
            } while (string.IsNullOrWhiteSpace(input));
            return input;
        }

        private static int LerInteiro(string prompt)
        {
            int valor;
            while (true)
            {
                Console.Write(prompt);
                string? line = Console.ReadLine() ?? "";
                if (int.TryParse(line, out valor))
                {
                    return valor;
                }
                PrintError("ERRO: Valor inválido. Digite apenas números.");
            }
        }
        
        private static DateTime LerDataHora(string prompt)
        {
            DateTime valor;
            while (true)
            {
                Console.Write(prompt);
                string? line = Console.ReadLine() ?? "";
                if (DateTime.TryParse(line, out valor))
                {
                    return valor;
                }
                PrintError("ERRO: Formato de data/hora inválido.");
            }
        }
    }
}