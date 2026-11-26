using PixGuard.Api.Dominio;
using PixGuard.Api.Infra;
using PixGuard.Api.Servicos;

namespace PixGuard.Tests;

/// <summary>Monta o cenário de teste: banco limpo, relógio fixo e uma conta com saldo.</summary>
public class Cenario
{
    public BancoEmMemoria Banco { get; } = new();
    public RelogioControlavel Relogio { get; } = new();
    public ContaService Contas { get; }
    public PixService Pix { get; }
    public Conta Conta { get; }

    public const string SenhaCorreta = "1234";

    public Cenario(int hora = 14, int minuto = 0)
    {
        Relogio.Fixar(Horario(hora, minuto));
        Contas = new ContaService(Banco, Relogio);
        Pix = new PixService(Banco, Relogio);

        Conta = new Conta
        {
            Cpf = "52998224725",
            Nome = "Maria Silva",
            DataNascimento = new DateOnly(1995, 4, 10),
            Senha = SenhaCorreta,
            Saldo = 5000m,
        };
        Banco.AdicionarConta(Conta);
    }

    // Horário de Brasília (UTC-3)
    public static DateTimeOffset Horario(int hora, int minuto = 0) =>
        new(2026, 11, 26, hora, minuto, 0, TimeSpan.FromHours(-3));

    public Resultado EnviarPix(decimal valor, string chave = "joao@email.com", string senha = SenhaCorreta, bool confirmarDuplicado = false) =>
        Pix.Enviar(Conta, new PixRequest(chave, valor, senha, confirmarDuplicado));
}

public static class ResultadoExtensions
{
    // O corpo é um objeto anônimo; lê a propriedade "codigo" por reflexão
    public static string? Codigo(this Resultado r) =>
        r.Corpo.GetType().GetProperty("codigo")?.GetValue(r.Corpo) as string;
}
