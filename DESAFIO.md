# Desafio Técnico — Desenvolvedor(a) Junior

## Centro de Treinamento Pokémon "Alto Nível"

Olá candidato(a),![alt text](https://static.wikia.nocookie.net/pokemongo/images/c/cd/Sticker_Funwari_Charmander.png/revision/latest?cb=20200817175607)

Primeiramente, parabéns por ter chegado até aqui! Essa tem sido uma Jornada Seletiva de altíssimo nível, mas o seu cadastro se destacou e não temos dúvidas de que você pode ser a pessoa certa para compor o nosso time.<br><br>
Abaixo, você encontrará todos as informações necessárias para realizar a sua Etapa de Task.<br>

Este desafio simula um problema real, com uma temática mais divertida.


O **Centro de Treinamento Alto Nível** é um serviço por assinatura onde Treinadores matriculam seus Pokémon em planos de treinamento mensais. Sua missão é construir o sistema de gestão dessas matrículas.

---

## 🎯 O que você vai construir

Uma aplicação com três partes:

1. **API REST em .NET** — gestão de Treinadores, Pokémon e Matrículas em planos
2. **Banco de dados SQL Server** — modelagem e uma consulta específica
3. **Frontend em Angular** — telas de listagem e cadastro

---

## 📋 Regras de negócio

### Entidades

- **Treinador**: nome, e-mail (único), cidade de origem.
- **Pokémon**: nome, tipo (ex.: Fogo, Água, Planta...), nível (1 a 100) e o Treinador dono.
- **Matrícula**: vincula um Pokémon a um **Plano de Treinamento**, com data de início, status (Ativa, Cancelada, Concluída) e valor mensal.

### Planos de Treinamento

| Plano | Valor mensal | Descrição |
|---|---|---|
| Ginásio Local | R$ 50,00 | Treinos básicos |
| Liga Regional | R$ 120,00 | Treinos intermediários + batalhas simuladas |
| Elite dos 4 | R$ 300,00 | Preparação completa para a Liga |

### Regras obrigatórias

**R1 — Matrícula única ativa:** um Pokémon **não pode** ter duas matrículas ativas ao mesmo tempo. A API deve rejeitar a tentativa com uma mensagem de erro clara, e o frontend deve exibir esse erro de forma amigável ao usuário.

**R2 — Upgrade com cálculo proporcional (pro-rata):** um Treinador pode fazer upgrade da matrícula de um Pokémon para um plano superior a qualquer momento do ciclo mensal. Quando isso acontece:

- A matrícula atual é encerrada e uma nova é criada no plano superior, iniciando na data do upgrade.
- O valor da **primeira cobrança do novo plano** deve ser proporcional aos dias restantes do ciclo, descontando o valor já pago não utilizado do plano antigo.
- **Exemplo:** um Pokémon está no plano Ginásio Local (R$ 50) e faz upgrade para Liga Regional (R$ 120) no dia 16 de um ciclo de 30 dias. Restam 15 dias. Crédito do plano antigo: R$ 50 × (15/30) = R$ 25. Custo do novo plano nos dias restantes: R$ 120 × (15/30) = R$ 60. **Primeira cobrança: R$ 60 − R$ 25 = R$ 35.**
- A API deve expor um endpoint de upgrade que retorna o valor calculado dessa primeira cobrança.
- Downgrade (plano inferior) **não é permitido** — a API deve rejeitar.

**R3 — Nível mínimo para a Elite dos 4:** apenas Pokémon de nível **50 ou superior** podem ser matriculados (ou receber upgrade) no plano Elite dos 4.

**R4 — Matrículas canceladas devem ser tratadas adequadamente nos cálculos e relatórios.**

**R5 — Se um Pokémon for transferido para outro Treinador, suas matrículas devem se comportar de forma coerente.**

> 💡 Se alguma regra parecer incompleta ou ambígua, você pode nos perguntar **ou** tomar uma decisão e documentá-la no README. As duas atitudes são bem-vindas — o que avaliamos é como você lida com isso.

---

## 🗄️ Parte SQL Server

Além da modelagem das tabelas (entregue o script de criação `schema.sql`), escreva **em SQL puro** (arquivo `consulta-mrr.sql`, sem ORM) a seguinte consulta:

> **Receita Mensal Recorrente (MRR) do Centro, agrupada por plano**, considerando apenas matrículas ativas, com uma linha de total geral ao final.

---

## 🖥️ Parte Angular

Telas mínimas:

1. **Listagem de matrículas** com busca por nome do Pokémon ou do Treinador e filtro por status.
2. **Formulário de nova matrícula** com validações (campos obrigatórios, nível mínimo para Elite dos 4).
3. **Fluxo de upgrade**: ao solicitar upgrade, exibir o valor da primeira cobrança retornado pela API antes de confirmar.
4. **Tratamento de erros da API** de forma amigável (ex.: tentativa de matrícula duplicada — R1).

Não avaliamos beleza visual. Avaliamos organização de componentes, validações e experiência básica de uso.

---

## 🤖 Uso de Inteligência Artificial

**O uso de IA (Copilot, ChatGPT, Claude etc.) é permitido e incentivado** — faz parte do nosso dia a dia. Pedimos apenas transparência: descreva no README como você usou (o que pediu, o que aproveitou, o que precisou corrigir ou reescrever).

Importante: haverá uma **sessão de conversa técnica** sobre a sua entrega, onde pediremos que você explique trechos do código e faça pequenas modificações ao vivo. Entregar código que você não compreende não vai te ajudar nessa etapa. 🙂

---

## 📦 Entrega

- O Envio deve ser feito por PullRequest com o nome completo do Candidato.
- Prazo: **5 dias corridos** a partir do recebimento. Estimamos algo entre 4 e 8 horas de trabalho — não é esperado que você use os 5 dias inteiros.
- O projeto deve rodar localmente com instruções claras no README.

### README obrigatório, contendo:

1. Instruções de execução (backend, frontend e scripts de banco).
2. Decisões técnicas e premissas assumidas (especialmente sobre R4 e R5).
3. O que você faria diferente ou melhoraria com mais tempo.
4. Como utilizou IA durante o desenvolvimento.

---

## ✅ O que avaliamos

- Corretude das regras de negócio, incluindo casos de borda (R1, R2, R3).
- Modelagem do banco e a consulta SQL solicitada.
- Organização do código no backend e no frontend.
- Clareza na comunicação: README, premissas documentadas, perguntas feitas.
- Na conversa técnica: compreensão do próprio código e raciocínio ao modificá-lo.


Quaisquer dúvidas técnicas em relação à Task, não deixe de entrar em contato com o e-mail: carlos.pedroni@aevo.com.br!

O nosso Time de Pessoas e Cultura se encontra também à disposição para quaisquer outras questões que achar relevante. Basta nos contatar no e-mail: rh@aevo.com.br!

Estes canais de comunicação estarão sempre abertos para você, não hesite em nos contatar caso tenha dúvidas.

Boa sorte! 🧡 ![alt text](https://static.wikia.nocookie.net/pokemongo/images/a/af/Sticker_Funwari_Bulbasaur_bye.png/revision/latest?cb=20200825201636)
