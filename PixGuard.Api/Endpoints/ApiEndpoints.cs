using PixGuard.Api.Infra;
using PixGuard.Api.Servicos;

namespace PixGuard.Api.Endpoints;

public static class ApiEndpoints
{
    private static IResult Responder(Resultado r) => Results.Json(r.Corpo, statusCode: r.Status);

    private static string? LerToken(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..].Trim() : null;
    }

    private static readonly IResult NaoAutenticado =
        Results.Json(new { codigo = "NAO_AUTENTICADO", mensagem = "Faça login para continuar" }, statusCode: 401);

    public static void MapearApi(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        // RN-01
        app.MapPost("/contas", (NovaContaRequest req, ContaService servico) =>
            Responder(servico.AbrirConta(req)));

        app.MapPost("/auth/login", (LoginRequest req, ContaService servico) =>
            Responder(servico.Login(req)));

        app.MapGet("/conta", (HttpRequest http, BancoEmMemoria banco) =>
        {
            var conta = banco.BuscarContaPorToken(LerToken(http));
            if (conta is null) return NaoAutenticado;

            return Results.Ok(new { conta.Nome, conta.Cpf, conta.Saldo, conta.PixBloqueado });
        });

        // RN-02, RN-03, RN-04
        app.MapPost("/pix", (PixRequest req, HttpRequest http, BancoEmMemoria banco, PixService servico) =>
        {
            var conta = banco.BuscarContaPorToken(LerToken(http));
            if (conta is null) return NaoAutenticado;

            return Responder(servico.Enviar(conta, req));
        });

        app.MapGet("/pix/{id}", (string id, HttpRequest http, BancoEmMemoria banco) =>
        {
            var conta = banco.BuscarContaPorToken(LerToken(http));
            if (conta is null) return NaoAutenticado;

            var transacao = conta.Transacoes.FirstOrDefault(t => t.Id == id);
            return transacao is null
                ? Results.Json(new { codigo = "PIX_NAO_ENCONTRADO", mensagem = "Transação não encontrada" }, statusCode: 404)
                : Results.Ok(transacao);
        });
    }

    /// <summary>
    /// Endpoints de apoio a testes. Só existem em ambiente de desenvolvimento/teste —
    /// em produção não são mapeados, então não há como alguém "mudar o relógio" do banco.
    /// </summary>
    public static void MapearApoioTestes(this WebApplication app)
    {
        var teste = app.MapGroup("/test");

        teste.MapPost("/reset", (BancoEmMemoria banco, RelogioControlavel relogio) =>
        {
            banco.Limpar();
            relogio.Liberar();
            return Results.NoContent();
        });

        teste.MapPut("/relogio", (FixarRelogioRequest req, RelogioControlavel relogio) =>
        {
            relogio.Fixar(req.DataHora);
            return Results.Ok(new { agora = relogio.Agora, fixo = true });
        });

        teste.MapPost("/relogio/avancar", (AvancarRelogioRequest req, RelogioControlavel relogio) =>
        {
            relogio.Avancar(TimeSpan.FromSeconds(req.Segundos));
            return Results.Ok(new { agora = relogio.Agora, fixo = relogio.EstaFixo });
        });

        teste.MapDelete("/relogio", (RelogioControlavel relogio) =>
        {
            relogio.Liberar();
            return Results.Ok(new { agora = relogio.Agora, fixo = false });
        });
    }
}

public record FixarRelogioRequest(DateTimeOffset DataHora);
public record AvancarRelogioRequest(int Segundos);
