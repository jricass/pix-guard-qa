namespace PixGuard.Api.Infra;

/// <summary>
/// Abstração do "agora". Design para testabilidade: em vez de chamar DateTime.Now
/// direto, as regras perguntam a hora ao relógio — e em testes o relógio pode ser controlado.
/// </summary>
public interface IRelogio
{
    DateTimeOffset Agora { get; }
}

/// <summary>
/// Usa o horário real de Brasília, mas permite fixar um horário em ambiente de teste
/// (ex.: "finja que são 21h" para testar o limite noturno às 3 da tarde).
/// </summary>
public class RelogioControlavel : IRelogio
{
    private static readonly TimeZoneInfo Brasilia =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    private DateTimeOffset? _horarioFixo;

    public DateTimeOffset Agora =>
        _horarioFixo ?? TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Brasilia);

    public bool EstaFixo => _horarioFixo.HasValue;

    public void Fixar(DateTimeOffset horario) => _horarioFixo = horario;

    public void Avancar(TimeSpan tempo)
    {
        if (_horarioFixo.HasValue)
            _horarioFixo = _horarioFixo.Value.Add(tempo);
    }

    public void Liberar() => _horarioFixo = null;
}
