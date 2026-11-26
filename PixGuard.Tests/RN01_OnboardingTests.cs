using PixGuard.Api.Dominio;
using PixGuard.Api.Servicos;

namespace PixGuard.Tests;

public class RN01_OnboardingTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")] // aceita CPF com máscara
    [InlineData("11144477735")]
    public void CPF_valido_e_aceito(string cpf) => Assert.True(Cpf.EhValido(cpf));

    [Theory]
    [InlineData("12345678901")] // CA-01.2: dígitos verificadores errados
    [InlineData("11111111111")] // CA-01.3: todos os dígitos iguais
    [InlineData("00000000000")]
    [InlineData("5299822472")]  // 10 dígitos
    [InlineData("")]
    [InlineData(null)]
    public void CPF_invalido_e_rejeitado(string? cpf) => Assert.False(Cpf.EhValido(cpf));

    [Fact]
    public void CA_01_1_conta_criada_com_dados_validos()
    {
        var c = new Cenario();
        var r = c.Contas.AbrirConta(new NovaContaRequest("João", "11144477735", new DateOnly(2000, 1, 1), "4321"));
        Assert.Equal(201, r.Status);
    }

    [Fact]
    public void CA_01_4_menor_de_idade_e_bloqueado()
    {
        var c = new Cenario(); // hoje = 26/11/2026
        var nascimento = new DateOnly(2008, 11, 27); // faz 18 amanhã → hoje tem 17
        var r = c.Contas.AbrirConta(new NovaContaRequest("João", "11144477735", nascimento, "4321"));

        Assert.Equal(422, r.Status);
        Assert.Equal("MENOR_DE_IDADE", r.Codigo());
    }

    [Fact]
    public void CA_01_5_quem_faz_18_hoje_pode_abrir_conta()
    {
        var c = new Cenario(); // hoje = 26/11/2026
        var nascimento = new DateOnly(2008, 11, 26);
        var r = c.Contas.AbrirConta(new NovaContaRequest("João", "11144477735", nascimento, "4321"));

        Assert.Equal(201, r.Status);
    }

    [Fact]
    public void CA_01_6_cpf_ja_cadastrado()
    {
        var c = new Cenario(); // o cenário já tem a conta 52998224725
        var r = c.Contas.AbrirConta(new NovaContaRequest("Outra", "52998224725", new DateOnly(1990, 1, 1), "4321"));

        Assert.Equal(409, r.Status);
        Assert.Equal("CPF_JA_CADASTRADO", r.Codigo());
    }
}
