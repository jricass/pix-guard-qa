namespace PixGuard.Api.Dominio;

/// <summary>
/// RN-01: validação de CPF pelos dígitos verificadores (módulo 11).
/// </summary>
public static class Cpf
{
    public static string Limpar(string? cpf) =>
        new string((cpf ?? string.Empty).Where(char.IsDigit).ToArray());

    public static bool EhValido(string? cpf)
    {
        var numeros = Limpar(cpf);

        if (numeros.Length != 11)
            return false;

        // CA-01.3: CPFs com todos os dígitos iguais passam no cálculo, mas são inválidos
        if (numeros.Distinct().Count() == 1)
            return false;

        var digitos = numeros.Select(c => c - '0').ToArray();

        return digitos[9] == CalcularDigito(digitos, 9)
            && digitos[10] == CalcularDigito(digitos, 10);
    }

    // Multiplica cada dígito por um peso decrescente, soma e aplica o módulo 11
    private static int CalcularDigito(int[] digitos, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += digitos[i] * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
