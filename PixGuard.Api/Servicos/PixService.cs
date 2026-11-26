using PixGuard.Api.Dominio;
using PixGuard.Api.Infra;

namespace PixGuard.Api.Servicos;

public record PixRequest(string? Chave, decimal? Valor, string? Senha, bool ConfirmarDuplicado = false);

public class PixService(BancoEmMemoria banco, IRelogio relogio)
{
    /// <summary>
    /// Ordem das validações (pensada como o fluxo do app):
    /// bloqueio → chave → valor → limite noturno → saldo → duplicidade → senha.
    /// A senha é a última etapa: o cliente só digita a senha depois que o PIX foi validado.
    /// </summary>
    public Resultado Enviar(Conta conta, PixRequest req)
    {
        lock (banco.Trava)
        {
            var agora = relogio.Agora;

            // CA-03.3: conta bloqueada não faz PIX
            if (conta.PixBloqueado)
                return Resultado.Erro(423, "PIX_BLOQUEADO", "Transações bloqueadas. Procure a central");

            var tipoChave = ChavePix.IdentificarTipo(req.Chave);
            if (tipoChave is null)
                return Resultado.Erro(422, "CHAVE_INVALIDA", "Chave PIX inválida");

            var valor = req.Valor ?? 0m;
            if (valor <= 0 || decimal.Round(valor, 2) != valor)
                return Resultado.Erro(422, "VALOR_INVALIDO", "Informe um valor maior que zero, com até 2 casas decimais");

            // RN-02: limite noturno (CA-02.1 a CA-02.5)
            if (RegrasPix.EhPeriodoNoturno(agora) && valor > RegrasPix.LimiteNoturno)
                return Resultado.Erro(422, "LIMITE_NOTURNO", "Valor acima do limite noturno (R$ 1.000,00)");

            if (valor > conta.Saldo)
                return Resultado.Erro(422, "SALDO_INSUFICIENTE", "Saldo insuficiente");

            // RN-04: mesmo valor para a mesma chave há menos de 2 minutos (CA-04.1, CA-04.4, CA-04.5)
            var chave = req.Chave!.Trim();
            var duplicado = conta.Transacoes.Any(t =>
                t.Chave == chave &&
                t.Valor == valor &&
                agora - t.DataHora < RegrasPix.JanelaDuplicidade);

            if (duplicado && !req.ConfirmarDuplicado)
                return Resultado.Erro(409, "PIX_DUPLICADO", "Você acabou de enviar este PIX. Deseja enviar novamente?");

            // RN-03: senha de transação (CA-03.1 a CA-03.4)
            if (req.Senha != conta.Senha)
            {
                conta.TentativasSenhaErradas++;

                if (conta.PixBloqueado)
                    return Resultado.Erro(423, "PIX_BLOQUEADO", "Transações bloqueadas. Procure a central");

                var restantes = Conta.MaxTentativasSenha - conta.TentativasSenhaErradas;
                return Resultado.Ok(403, new
                {
                    codigo = "SENHA_INCORRETA",
                    mensagem = $"Senha incorreta. Restam {restantes} tentativa{(restantes > 1 ? "s" : "")}",
                    tentativasRestantes = restantes,
                });
            }

            conta.TentativasSenhaErradas = 0; // CA-03.4: acertou, zera o contador
            conta.Saldo -= valor;

            var transacao = new TransacaoPix(
                Id: $"E{agora:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                Chave: chave,
                TipoChave: tipoChave,
                Valor: valor,
                DataHora: agora);
            conta.Transacoes.Add(transacao);

            return Resultado.Ok(201, new
            {
                transacao.Id,
                transacao.Chave,
                transacao.TipoChave,
                transacao.Valor,
                transacao.DataHora,
                status = "CONCLUIDO",
                saldoAtual = conta.Saldo,
            });
        }
    }
}
