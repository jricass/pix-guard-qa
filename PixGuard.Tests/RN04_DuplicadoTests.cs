namespace PixGuard.Tests;

public class RN04_DuplicadoTests
{
    [Fact]
    public void CA_04_1_mesmo_pix_em_menos_de_2_minutos_pede_confirmacao()
    {
        var c = new Cenario();
        c.EnviarPix(50m, chave: "maria@email.com");
        c.Relogio.Avancar(TimeSpan.FromSeconds(30));

        var r = c.EnviarPix(50m, chave: "maria@email.com");

        Assert.Equal(409, r.Status);
        Assert.Equal("PIX_DUPLICADO", r.Codigo());
    }

    [Fact]
    public void CA_04_2_confirmando_o_segundo_pix_e_enviado()
    {
        var c = new Cenario();
        c.EnviarPix(50m, chave: "maria@email.com");

        var r = c.EnviarPix(50m, chave: "maria@email.com", confirmarDuplicado: true);

        Assert.Equal(201, r.Status);
        Assert.Equal(4900m, c.Conta.Saldo);
    }

    [Fact]
    public void CA_04_3_sem_confirmar_o_saldo_nao_muda()
    {
        var c = new Cenario();
        c.EnviarPix(50m, chave: "maria@email.com");
        c.EnviarPix(50m, chave: "maria@email.com"); // bloqueado como duplicado

        Assert.Equal(4950m, c.Conta.Saldo);
    }

    [Fact]
    public void CA_04_4_mesmo_valor_para_outra_chave_nao_e_duplicado()
    {
        var c = new Cenario();
        c.EnviarPix(50m, chave: "maria@email.com");

        var r = c.EnviarPix(50m, chave: "pedro@email.com");

        Assert.Equal(201, r.Status);
    }

    [Fact]
    public void CA_04_5_depois_de_2_minutos_nao_e_duplicado()
    {
        var c = new Cenario();
        c.EnviarPix(50m, chave: "maria@email.com");
        c.Relogio.Avancar(TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(1)));

        var r = c.EnviarPix(50m, chave: "maria@email.com");

        Assert.Equal(201, r.Status);
    }
}
