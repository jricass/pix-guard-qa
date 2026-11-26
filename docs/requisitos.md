# Requisitos — PIX Guard

Regras de negócio do app. Cada critério de aceite (CA) vira pelo menos um caso de teste.

## RN-01 — Abertura de conta (onboarding)

**História:** Como pessoa interessada, quero abrir uma conta informando meus dados, para começar a usar o banco.

**Critérios de aceite:**

- **CA-01.1** Dado um CPF válido e idade ≥ 18 anos, quando envio o cadastro, então a conta é criada.
- **CA-01.2** Dado um CPF com dígitos verificadores inválidos (ex.: `12345678901`), quando envio o cadastro, então vejo "CPF inválido".
- **CA-01.3** Dado um CPF com todos os dígitos iguais (ex.: `11111111111`), quando envio o cadastro, então vejo "CPF inválido".
- **CA-01.4** Dado que tenho 17 anos, quando envio o cadastro, então vejo "É necessário ter 18 anos ou mais".
- **CA-01.5** Dado que faço 18 anos hoje, quando envio o cadastro, então a conta é criada.
- **CA-01.6** Dado um CPF já cadastrado, quando envio o cadastro, então vejo "CPF já possui conta".

## RN-02 — Limite noturno do PIX

**História:** Como cliente, quero que meu limite de PIX seja menor à noite, para reduzir prejuízo em caso de golpe ou sequestro relâmpago.

> Base: regra do Banco Central — período noturno das **20h às 6h**, limite padrão de **R$ 1.000,00** por transação.

**Critérios de aceite:**

- **CA-02.1** Dado que são 21h, quando envio PIX de R$ 1.000,00, então o PIX é concluído.
- **CA-02.2** Dado que são 21h, quando envio PIX de R$ 1.000,01, então vejo "Valor acima do limite noturno (R$ 1.000,00)".
- **CA-02.3** Dado que são 14h, quando envio PIX de R$ 1.000,01, então o PIX é concluído (respeitando o saldo).
- **CA-02.4** Dado que são exatamente 20h00, quando envio PIX de R$ 1.000,01, então o limite noturno já se aplica.
- **CA-02.5** Dado que são exatamente 6h00, quando envio PIX de R$ 1.000,01, então o PIX é concluído (limite diurno).

## RN-03 — Confirmação por senha de transação

**História:** Como cliente, quero confirmar cada PIX com uma senha, para que ninguém envie dinheiro com meu celular desbloqueado.

**Critérios de aceite:**

- **CA-03.1** Dado um PIX válido, quando digito a senha correta, então o PIX é concluído e vejo o comprovante.
- **CA-03.2** Dado um PIX válido, quando digito a senha errada, então vejo "Senha incorreta. Restam 2 tentativas" e o PIX não é enviado.
- **CA-03.3** Dado que errei a senha 3 vezes seguidas, então a conta é bloqueada para PIX e vejo "Transações bloqueadas. Procure a central".
- **CA-03.4** Dado que errei 2 vezes e acertei na 3ª, então o PIX é concluído e o contador de erros volta a zero.

## RN-04 — Prevenção de PIX duplicado

**História:** Como cliente, quero ser avisado se tentar enviar o mesmo PIX duas vezes seguidas, para não pagar em dobro por engano.

**Critérios de aceite:**

- **CA-04.1** Dado que enviei R$ 50,00 para `maria@email.com`, quando tento enviar R$ 50,00 para a mesma chave em menos de 2 minutos, então vejo "Você acabou de enviar este PIX. Deseja enviar novamente?".
- **CA-04.2** Dado o aviso de duplicidade, quando confirmo, então o segundo PIX é concluído.
- **CA-04.3** Dado o aviso de duplicidade, quando cancelo, então nenhum PIX novo é enviado e o saldo não muda.
- **CA-04.4** Dado que enviei R$ 50,00 para `maria@email.com`, quando envio R$ 50,00 para **outra** chave, então não há aviso.
- **CA-04.5** Dado que enviei R$ 50,00 para `maria@email.com` há mais de 2 minutos, quando envio de novo, então não há aviso.
