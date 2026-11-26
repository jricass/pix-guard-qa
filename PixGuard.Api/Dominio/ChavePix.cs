using System.Text.RegularExpressions;

namespace PixGuard.Api.Dominio;

/// <summary>
/// Identifica o tipo da chave PIX pelo formato, como os apps de banco fazem.
/// </summary>
public static class ChavePix
{
    public static string? IdentificarTipo(string? chave)
    {
        var c = (chave ?? string.Empty).Trim();

        if (Regex.IsMatch(c, @"^\d{11}$"))
            return Cpf.EhValido(c) ? "CPF" : null;

        if (Regex.IsMatch(c, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            return "E-mail";

        if (Regex.IsMatch(c, @"^\+55\d{10,11}$"))
            return "Telefone";

        if (Guid.TryParseExact(c, "D", out _))
            return "Aleatória";

        return null;
    }
}
