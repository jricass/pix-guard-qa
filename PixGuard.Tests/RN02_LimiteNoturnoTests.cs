namespace PixGuard.Tests;

/// <summary>
/// Análise de valor-limite em duas dimensões: o VALOR (1.000,00 / 1.000,01)
/// e o HORÁRIO (05:59 / 06:00 / 19:59 / 20:00).
/// </summary>
public class RN02_LimiteNoturnoTests
{
    [Fact]
    public void CA_02_1_noturno_no_limite_exato_passa()
    {
        var r = new Cenario(hora: 21).EnviarPix(1000.00m);
        Assert.Equal(201, r.Status);
    }

    [Fact]
    public void CA_02_2_noturno_um_centavo_acima_e_bloqueado()
    {
        var r = new Cenario(hora: 21).EnviarPix(1000.01m);
        Assert.Equal(422, r.Status);
        Assert.Equal("LIMITE_NOTURNO", r.Codigo());
    }

    [Fact]
    public void CA_02_3_diurno_acima_de_mil_passa()
    {
        var r = new Cenario(hora: 14).EnviarPix(1000.01m);
        Assert.Equal(201, r.Status);
    }

    [Theory]
    [InlineData(20, 0, 422)]  // CA-02.4: 20:00 em ponto já é noturno
    [InlineData(19, 59, 201)] // um minuto antes ainda é diurno
    [InlineData(6, 0, 201)]   // CA-02.5: 06:00 em ponto já é diurno
    [InlineData(5, 59, 422)]  // um minuto antes ainda é noturno
    [InlineData(0, 0, 422)]   // meia-noite
    public void Fronteiras_de_horario(int hora, int minuto, int statusEsperado)
    {
        var r = new Cenario(hora, minuto).EnviarPix(1000.01m);
        Assert.Equal(statusEsperado, r.Status);
    }
}
