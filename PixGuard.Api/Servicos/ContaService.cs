using PixGuard.Api.Dominio;
using PixGuard.Api.Infra;

namespace PixGuard.Api.Servicos;

public record NovaContaRequest(string? Nome, string? Cpf, DateOnly? DataNascimento, string? Senha);
public record LoginRequest(string? Cpf, string? Senha);

/// <summary>Resultado padrão das operações: sucesso com dados ou erro com código e mensagem.</summary>
public record Resultado(int Status, object Corpo)
{
    public static Resultado Ok(int status, object corpo) => new(status, corpo);
    public static Resultado Erro(int status, string codigo, string mensagem) =>
        new(status, new { codigo, mensagem });
}

// Valida conta: nome, cpf, idade, senah, etc. E Login, devolve token
public class ContaService(BancoEmMemoria banco, IRelogio relogio)
{
    // RN-01 — Abertura de conta
    public Resultado AbrirConta(NovaContaRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Resultado.Erro(422, "NOME_OBRIGATORIO", "Informe o nome completo");

        if (!Cpf.EhValido(req.Cpf))
            return Resultado.Erro(422, "CPF_INVALIDO", "CPF inválido"); // CA-01.2 e CA-01.3

        if (req.DataNascimento is null)
            return Resultado.Erro(422, "DATA_NASCIMENTO_OBRIGATORIA", "Informe a data de nascimento");

        var hoje = DateOnly.FromDateTime(relogio.Agora.DateTime);
        if (RegrasPix.CalcularIdade(req.DataNascimento.Value, hoje) < RegrasPix.IdadeMinima)
            return Resultado.Erro(422, "MENOR_DE_IDADE", "É necessário ter 18 anos ou mais"); // CA-01.4

        if (req.Senha is null || req.Senha.Length != 4 || !req.Senha.All(char.IsDigit))
            return Resultado.Erro(422, "SENHA_INVALIDA", "A senha deve ter 4 dígitos numéricos");

        var cpf = Cpf.Limpar(req.Cpf);

        lock (banco.Trava)
        {
            if (banco.ExisteConta(cpf))
                return Resultado.Erro(409, "CPF_JA_CADASTRADO", "CPF já possui conta"); // CA-01.6

            var conta = new Conta
            {
                Cpf = cpf,
                Nome = req.Nome.Trim(),
                DataNascimento = req.DataNascimento.Value,
                Senha = req.Senha,
                Saldo = RegrasPix.SaldoInicialContaDemo,
            };
            banco.AdicionarConta(conta);

            return Resultado.Ok(201, new { conta.Cpf, conta.Nome, conta.Saldo }); // CA-01.1 e CA-01.5
        }
    }

    public Resultado Login(LoginRequest req)
    {
        var conta = banco.BuscarConta(Cpf.Limpar(req.Cpf));
        if (conta is null || conta.Senha != req.Senha)
            return Resultado.Erro(401, "CREDENCIAIS_INVALIDAS", "CPF ou senha inválidos");

        var token = banco.CriarSessao(conta.Cpf);
        return Resultado.Ok(200, new { token, conta.Nome });
    }
}
