using PixGuard.Api.Dominio;

namespace PixGuard.Api.Infra;

/// <summary>
/// "Banco de dados" em memória. Suficiente para um ambiente de testes:
/// reinicia a cada execução e pode ser zerado via /test/reset.
/// </summary>
public class BancoEmMemoria
{
    private readonly Dictionary<string, Conta> _contas = new();
    private readonly Dictionary<string, string> _sessoes = new(); // token -> cpf

    public object Trava { get; } = new();

    public bool ExisteConta(string cpf) => _contas.ContainsKey(cpf);

    public void AdicionarConta(Conta conta) => _contas[conta.Cpf] = conta;

    public Conta? BuscarConta(string cpf) => _contas.GetValueOrDefault(cpf);

    public string CriarSessao(string cpf)
    {
        var token = Guid.NewGuid().ToString("N");
        _sessoes[token] = cpf;
        return token;
    }

    public Conta? BuscarContaPorToken(string? token) =>
        token is not null && _sessoes.TryGetValue(token, out var cpf) ? BuscarConta(cpf) : null;

    public void Limpar()
    {
        _contas.Clear();
        _sessoes.Clear();
    }
}
