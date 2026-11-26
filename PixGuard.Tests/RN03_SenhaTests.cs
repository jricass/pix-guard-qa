namespace PixGuard.Tests;

public class RN03_SenhaTests
{
    [Fact]
    public void CA_03_1_senha_correta_conclui_e_debita()
    {
        var c = new Cenario();
        var r = c.EnviarPix(100m);

        Assert.Equal(201, r.Status);
        Assert.Equal(4900m, c.Conta.Saldo);
    }

    [Fact]
    public void CA_03_2_senha_errada_nao_envia_e_informa_tentativas()
    {
        var c = new Cenario();
        var r = c.EnviarPix(100m, senha: "0000");

        Assert.Equal(403, r.Status);
        Assert.Equal("SENHA_INCORRETA", r.Codigo());
        Assert.Equal(5000m, c.Conta.Saldo); // nada foi debitado
    }

    [Fact]
    public void CA_03_3_tres_erros_bloqueiam_a_conta()
    {
        var c = new Cenario();
        c.EnviarPix(100m, senha: "0000");
        c.EnviarPix(100m, senha: "0000");
        var terceira = c.EnviarPix(100m, senha: "0000");

        Assert.Equal(423, terceira.Status);
        Assert.True(c.Conta.PixBloqueado);

        // Bloqueada, nem a senha certa funciona
        var comSenhaCerta = c.EnviarPix(100m);
        Assert.Equal(423, comSenhaCerta.Status);
    }

    [Fact]
    public void CA_03_4_acertar_na_terceira_zera_o_contador()
    {
        var c = new Cenario();
        c.EnviarPix(100m, senha: "0000");
        c.EnviarPix(100m, senha: "0000");
        var r = c.EnviarPix(100m);

        Assert.Equal(201, r.Status);
        Assert.Equal(0, c.Conta.TentativasSenhaErradas);
    }
}
