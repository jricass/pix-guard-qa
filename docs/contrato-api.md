# Contrato da API — PIX Guard

Base local: `http://localhost:5080`. Respostas de erro sempre no formato:

```json
{ "codigo": "LIMITE_NOTURNO", "mensagem": "Valor acima do limite noturno (R$ 1.000,00)" }
```

> Teste pelo **`codigo`**, não pela `mensagem`: o texto pode mudar (tradução, ajuste de UX), o código é o contrato.

## Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/health` | — | API no ar |
| POST | `/contas` | — | Abrir conta (RN-01). Conta demo nasce com R$ 5.000,00 |
| POST | `/auth/login` | — | Retorna `token` |
| GET | `/conta` | Bearer | Nome, CPF, saldo, `pixBloqueado` |
| POST | `/pix` | Bearer | Enviar PIX (RN-02, RN-03, RN-04) |
| GET | `/pix/{id}` | Bearer | Consultar comprovante |

### Corpos

`POST /contas`
```json
{ "nome": "Maria Silva", "cpf": "52998224725", "dataNascimento": "1995-04-10", "senha": "1234" }
```

`POST /pix`
```json
{ "chave": "joao@email.com", "valor": 150.00, "senha": "1234", "confirmarDuplicado": false }
```

## Códigos de resposta

| Status | `codigo` | Quando | Regra |
|---|---|---|---|
| 201 | — | Conta criada / PIX concluído | — |
| 401 | `NAO_AUTENTICADO` | Sem token ou token inválido | — |
| 401 | `CREDENCIAIS_INVALIDAS` | Login com CPF/senha errados | — |
| 403 | `SENHA_INCORRETA` | Senha de transação errada (traz `tentativasRestantes`) | RN-03 |
| 409 | `CPF_JA_CADASTRADO` | CPF já tem conta | RN-01 |
| 409 | `PIX_DUPLICADO` | Mesmo valor e chave há < 2 min | RN-04 |
| 422 | `CPF_INVALIDO` | Dígitos verificadores errados ou todos iguais | RN-01 |
| 422 | `MENOR_DE_IDADE` | Menos de 18 anos | RN-01 |
| 422 | `CHAVE_INVALIDA` | Chave PIX em formato inválido | — |
| 422 | `VALOR_INVALIDO` | Valor ≤ 0 ou com mais de 2 casas | — |
| 422 | `LIMITE_NOTURNO` | > R$ 1.000,00 entre 20h e 6h | RN-02 |
| 422 | `SALDO_INSUFICIENTE` | Valor maior que o saldo | — |
| 423 | `PIX_BLOQUEADO` | 3 senhas erradas | RN-03 |

**Ordem das validações no PIX:** bloqueio → chave → valor → limite noturno → saldo → duplicidade → senha.

## Apoio a testes (só em ambiente Development)

| Método | Rota | Corpo | Uso |
|---|---|---|---|
| POST | `/test/reset` | — | Zera contas, sessões e relógio |
| PUT | `/test/relogio` | `{ "dataHora": "2026-11-26T21:00:00-03:00" }` | Fixa o horário |
| POST | `/test/relogio/avancar` | `{ "segundos": 121 }` | Avança o relógio fixo (testar a janela de 2 min) |
| DELETE | `/test/relogio` | — | Volta ao horário real |

Esses endpoints **não existem em produção**: o `Program.cs` só os mapeia quando `ASPNETCORE_ENVIRONMENT=Development`.
