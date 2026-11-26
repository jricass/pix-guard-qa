namespace PixGuard.Api.Dominio;

/// <summary>
/// Parâmetros das regras de negócio. Centralizados para facilitar manutenção e testes.
/// </summary>
public static class RegrasPix
{
    // RN-01
    public const int IdadeMinima = 18;
    public const decimal SaldoInicialContaDemo = 5000.00m;

    // RN-02: período noturno do Banco Central (20h às 6h) e limite padrão
    public const int InicioNoturno = 20;
    public const int FimNoturno = 6;
    public const decimal LimiteNoturno = 1000.00m;

    // RN-04: janela para considerar um PIX como possível duplicado
    public static readonly TimeSpan JanelaDuplicidade = TimeSpan.FromMinutes(2);

    /// <summary>20:00 já é noturno; 06:00 já é diurno.</summary>
    public static bool EhPeriodoNoturno(DateTimeOffset horario) =>
        horario.Hour >= InicioNoturno || horario.Hour < FimNoturno;

    public static int CalcularIdade(DateOnly nascimento, DateOnly hoje)
    {
        var idade = hoje.Year - nascimento.Year;
        if (nascimento > hoje.AddYears(-idade))
            idade--;
        return idade;
    }
}
