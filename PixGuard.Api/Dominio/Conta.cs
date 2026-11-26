namespace PixGuard.Api.Dominio;

public class Conta
{
    public const int MaxTentativasSenha = 3;

    public required string Cpf { get; init; }
    public required string Nome { get; init; }
    public required DateOnly DataNascimento { get; init; }
    public required string Senha { get; init; }
    public decimal Saldo { get; set; }

    // RN-03: controle de tentativas de senha de transação
    public int TentativasSenhaErradas { get; set; }
    public bool PixBloqueado => TentativasSenhaErradas >= MaxTentativasSenha;

    public List<TransacaoPix> Transacoes { get; } = new();
}

public record TransacaoPix(
    string Id,
    string Chave,
    string TipoChave,
    decimal Valor,
    DateTimeOffset DataHora);
