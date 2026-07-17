# Feature Specification: Gestão de Matrículas do Centro de Treinamento Pokémon

**Feature Branch**: N/A — sem hook `before_specify` configurado; desenvolvimento permanece na branch atual (`livia-gomes`)

**Created**: 2026-07-17

**Status**: Draft

**Input**: User description: "Gestão de Matrículas (completa) — especificação única cobrindo Treinador, Pokémon, Plano e Matrícula, incluindo R1 (matrícula única ativa), R2 (upgrade pro-rata), R3 (nível mínimo Elite dos 4), R4 (cancelamentos) e R5 (transferência de Pokémon) — todo o núcleo do desafio (DESAFIO.md)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Matricular Pokémon em Plano de Treinamento (Priority: P1)

Um Treinador cadastra um Pokémon e o matricula em um dos três planos de
treinamento (Ginásio Local, Liga Regional, Elite dos 4); a data de início é
definida automaticamente pelo sistema no momento da confirmação (a data da
contratação), refletindo o instante em que o pagamento seria processado.

**Why this priority**: sem essa capacidade nenhuma outra funcionalidade do
sistema tem dados para operar — é o caminho que cria o registro central
(Matrícula) do domínio.

**Independent Test**: pode ser testado sozinho cadastrando um Treinador, um
Pokémon e criando uma matrícula em um plano; a matrícula deve aparecer no
estado ativo, sem data de término, com o valor mensal correto.

**Acceptance Scenarios**:

1. **Given** um Treinador e um Pokémon de nível 30 cadastrados, **When** o
   Treinador matricula o Pokémon no plano Ginásio Local, **Then** a matrícula
   é criada sem data de término (ativa), com a data de início definida
   automaticamente como o momento da confirmação e valor mensal de R$ 50,00.
2. **Given** um Pokémon já matriculado e Ativo no plano Liga Regional,
   **When** o Treinador tenta criar uma nova matrícula para o mesmo Pokémon
   em qualquer plano, **Then** o sistema rejeita a operação com uma mensagem
   de erro clara informando que o Pokémon já possui matrícula ativa (R1).
3. **Given** um Pokémon de nível 40, **When** o Treinador tenta matriculá-lo
   no plano Elite dos 4, **Then** o sistema rejeita a operação informando que
   o nível mínimo exigido é 50 (R3).
4. **Given** um Pokémon de nível 50, **When** o Treinador o matricula no
   plano Elite dos 4, **Then** a matrícula é criada normalmente (nível limite
   é aceito).

---

### User Story 2 - Upgrade de Matrícula com Cobrança Pro-Rata (Priority: P2)

Um Treinador solicita o upgrade da matrícula de um Pokémon para um plano
superior a qualquer momento do ciclo mensal, e o sistema calcula e exibe o
valor da primeira cobrança do novo plano antes de confirmar.

**Why this priority**: é a regra de negócio com maior complexidade de
cálculo do desafio e a que mais concentra risco de erro (R2); depende de já
existir uma matrícula ativa (User Story 1).

**Independent Test**: pode ser testado isoladamente criando uma matrícula
ativa em um plano inferior e disparando um upgrade em uma data conhecida do
ciclo, verificando se o valor da primeira cobrança calculado bate com o
esperado.

**Acceptance Scenarios**:

1. **Given** uma matrícula ativa no plano Ginásio Local (R$ 50,00) desde o
   início de um ciclo de 30 dias, **When** o Treinador solicita upgrade para
   o plano Liga Regional (R$ 120,00) no dia 16 do ciclo, **Then** o sistema
   apresenta o valor da primeira cobrança de R$ 35,00 antes de confirmar
   (crédito de R$ 25,00 do plano antigo descontado de R$ 60,00 do novo
   plano nos 15 dias restantes).
2. **Given** o Treinador confirmou o upgrade do cenário anterior, **When** a
   confirmação é processada, **Then** a data de término da matrícula antiga é
   definida como a data do upgrade (passando a contar como encerrada) e uma
   nova matrícula ativa (sem data de término) no plano Liga Regional é criada
   com início na data do upgrade.
3. **Given** uma matrícula ativa no plano Liga Regional, **When** o Treinador
   tenta fazer "upgrade" para o plano Ginásio Local (plano inferior), **Then**
   o sistema rejeita a operação com uma mensagem informando que downgrade não
   é permitido.
4. **Given** uma matrícula ativa de um Pokémon de nível 45, **When** o
   Treinador solicita upgrade para o plano Elite dos 4, **Then** o sistema
   rejeita a operação informando o nível mínimo exigido (R3 também se aplica
   a upgrades).
5. **Given** uma matrícula ativa iniciada em 31 de janeiro, **When** o
   Treinador solicita upgrade em fevereiro (mês sem dia 31), **Then** o
   sistema calcula o pro-rata usando um ciclo que termina em 28 ou 29 de
   fevereiro (conforme o ano), sem erro e sem assumir um ciclo fixo de 30
   dias.

---

### User Story 3 - Consultar e Filtrar Matrículas (Priority: P3)

Qualquer usuário do sistema visualiza a lista de matrículas, podendo buscar
por nome do Pokémon ou do Treinador e filtrar por status (Ativa, Ativa "a
encerrar", Encerrada).

**Why this priority**: é uma das telas mínimas exigidas e a forma primária de
o usuário verificar o resultado das ações das demais histórias.

**Independent Test**: pode ser testado com uma massa de matrículas em
diferentes status, verificando se busca por nome e filtro por status
retornam os subconjuntos corretos.

**Acceptance Scenarios**:

1. **Given** matrículas de Pokémon com nomes diferentes, **When** o usuário
   busca por parte do nome de um Pokémon, **Then** apenas as matrículas
   desse Pokémon são exibidas.
2. **Given** matrículas de diferentes Treinadores, **When** o usuário busca
   por parte do nome de um Treinador, **Then** apenas as matrículas dos
   Pokémon desse Treinador são exibidas.
3. **Given** matrículas em estado Ativa, Ativa "a encerrar" e Encerrada,
   **When** o usuário filtra por status Ativa, **Then** somente matrículas
   sem data de término ou com data de término igual/posterior a hoje são
   exibidas.
4. **Given** nenhuma matrícula corresponde à busca, **When** o usuário
   pesquisa por um termo inexistente, **Then** a lista é exibida vazia com
   uma indicação clara de "nenhum resultado", sem erro.

---

### User Story 4 - Cancelar Matrícula e Transferir Pokémon entre Treinadores (Priority: P4)

Um Treinador cancela a matrícula ativa de um Pokémon, e um Pokémon pode ser
transferido diretamente para outro Treinador, sem necessidade de aceite da
outra parte. Se o Pokémon tiver matrícula ativa, ela é encerrada
automaticamente sob o Treinador de origem e uma nova matrícula equivalente é
criada automaticamente sob o Treinador de destino, sem exigir uma ação manual
adicional para o Pokémon continuar treinando.

**Why this priority**: cobre R4 e R5; depende das histórias anteriores
existirem para ter o que cancelar/transferir.

**Independent Test**: pode ser testado cancelando uma matrícula ativa e
verificando que ela some dos cálculos de receita ativa, e transferindo um
Pokémon com matrícula ativa entre dois Treinadores, verificando que a
matrícula anterior é encerrada na data da transferência, permanece no
histórico, e uma nova matrícula equivalente é criada automaticamente sob o
novo Treinador.

**Acceptance Scenarios**:

1. **Given** uma matrícula Ativa, **When** o Treinador a cancela, **Then** a
   data de término é definida como o fim do ciclo mensal pago vigente (sem
   estorno do valor já pago), o Pokémon mantém acesso ao treinamento até essa
   data, e a matrícula deixa de contar como ativa em qualquer cálculo de
   receita ativa assim que essa data é ultrapassada (R4).
2. **Given** uma matrícula com data de término no passado, **When** o mesmo
   Pokémon é matriculado novamente em qualquer plano, **Then** a nova
   matrícula é criada normalmente (uma matrícula encerrada não bloqueia
   novas matrículas; R1 só impede duas matrículas simultaneamente ativas).
3. **Given** um Pokémon com matrícula ativa no plano X pertencente ao
   Treinador A, **When** o Treinador A transfere o Pokémon para o Treinador
   B (sem necessidade de aceite do Treinador B), **Then** a matrícula sob o
   Treinador A é encerrada imediatamente com data de término igual à data da
   transferência (refletindo exatamente o período em que o Pokémon esteve
   sob o Treinador A, sem estorno do valor já pago), o registro permanece no
   histórico para fins de auditoria, a propriedade do Pokémon passa a ser do
   Treinador B, e o sistema cria automaticamente uma nova matrícula ativa no
   mesmo plano X sob o Treinador B, com início na data da transferência e
   cobrança integral do valor mensal do plano (sem pro-rata, por se tratar de
   um novo ciclo de cobrança).
4. **Given** um Pokémon sem matrícula ativa pertencente ao Treinador A,
   **When** o Treinador A o transfere para o Treinador B, **Then** a
   propriedade do Pokémon passa para o Treinador B sem nenhum efeito sobre
   matrículas (não há matrícula ativa para encerrar ou recriar).

---

### User Story 5 - Relatório de Receita Mensal Recorrente (MRR) por Plano (Priority: P5)

Uma pessoa com acesso ao banco de dados consulta a Receita Mensal Recorrente
do Centro, agrupada por plano, considerando apenas matrículas ativas, com uma
linha de total geral ao final.

**Why this priority**: é um entregável explícito e avaliado separadamente
(consulta SQL), mas não bloqueia nenhuma das histórias de uso do sistema por
um Treinador.

**Independent Test**: pode ser testado populando matrículas em múltiplos
planos com datas de término variadas e conferindo se o total por plano e o
total geral somam apenas o valor mensal das matrículas ativas.

**Acceptance Scenarios**:

1. **Given** matrículas ativas em mais de um plano e algumas já encerradas,
   **When** a consulta de MRR é executada, **Then** o resultado mostra uma
   linha por plano com a soma dos valores mensais apenas das matrículas
   ativas daquele plano, e uma linha final com o total geral.
2. **Given** um plano sem nenhuma matrícula ativa, **When** a consulta é
   executada, **Then** esse plano não aparece com uma linha própria (ver
   Assumptions), mas seu valor de R$ 0,00 está implicitamente refletido por
   sua ausência.

### Edge Cases

- Tentativa de criar uma segunda matrícula Ativa para um Pokémon que já
  possui uma (R1) — deve ser rejeitada com mensagem clara.
- Upgrade solicitado no primeiro dia do ciclo (dias restantes = ciclo
  inteiro) e no último dia do ciclo (dias restantes próximos de zero) — o
  cálculo pro-rata deve continuar consistente nos extremos.
- Tentativa de matricular ou fazer upgrade para Elite dos 4 com Pokémon de
  nível abaixo de 50 (R3), incluindo o valor limite (nível exatamente 50,
  que deve ser aceito).
- Tentativa de downgrade de plano (R2) — deve ser rejeitada.
- Cadastro de Treinador com e-mail já usado por outro Treinador — deve ser
  rejeitado.
- Cadastro de Pokémon com nível fora do intervalo 1-100 — deve ser rejeitado.
- Cadastro de Pokémon com tipo fora da lista fixa de 18 tipos — deve ser
  rejeitado (ex.: valor livre digitado pelo usuário).
- Busca na listagem de matrículas sem nenhum resultado correspondente.
- Transferência de um Pokémon para o mesmo Treinador que já é seu dono (deve
  ser tratada como no-op ou rejeitada, não como erro do sistema).
- Transferência de um Pokémon com matrícula ativa: a matrícula sob o
  Treinador de origem deve ser encerrada com data de término igual à data da
  transferência, mesmo que essa data caia no meio do ciclo mensal pago, e uma
  nova matrícula equivalente deve ser criada automaticamente sob o Treinador
  de destino, cobrada integralmente a partir dessa data (R5).
- Matrícula com data de término igual a hoje deve ainda contar como ativa
  (o corte para "encerrada" é estritamente no dia seguinte).
- Cálculo pro-rata do upgrade em um mês com 28, 29, 30 ou 31 dias — o
  denominador do cálculo (duração do ciclo) deve refletir o número real de
  dias entre o início do ciclo e seu término, não um valor fixo (R2).
- Matrícula iniciada em um dia que não existe todo mês (29, 30 ou 31): o
  ciclo seguinte MUST terminar no último dia do mês em que esse dia não
  existir (ex.: matrícula iniciada em 31/01 tem ciclo terminando em 28/02, ou
  29/02 em ano bissexto), sem gerar erro ou pular o ciclo (R2).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir cadastrar um Treinador com nome,
  e-mail (único) e cidade de origem.
- **FR-002**: O sistema MUST rejeitar o cadastro de um Treinador cujo e-mail
  já esteja em uso por outro Treinador.
- **FR-003**: O sistema MUST permitir cadastrar um Pokémon com nome, tipo,
  nível (entre 1 e 100) e o Treinador dono.
- **FR-004**: O sistema MUST rejeitar o cadastro de um Pokémon com nível fora
  do intervalo 1-100.
- **FR-005**: O sistema MUST permitir matricular um Pokémon em um dos três
  planos de treinamento (Ginásio Local, Liga Regional, Elite dos 4),
  registrando a data de início como o momento da confirmação (não um campo
  editável pelo usuário), nenhuma data de término (matrícula ativa) e o valor
  mensal do plano.
- **FR-006**: O sistema MUST rejeitar a criação de uma matrícula ativa para
  um Pokémon que já possua outra matrícula ativa — sem data de término, ou
  com data de término igual/posterior a hoje (R1) —, retornando uma mensagem
  de erro clara e amigável.
- **FR-007**: O sistema MUST rejeitar a matrícula ou o upgrade de um Pokémon
  de nível inferior a 50 no plano Elite dos 4 (R3).
- **FR-008**: O sistema MUST permitir solicitar o upgrade da matrícula ativa
  de um Pokémon para um plano de valor mensal superior, a qualquer momento do
  ciclo mensal corrente.
- **FR-009**: Ao processar um upgrade, o sistema MUST calcular o valor da
  primeira cobrança do novo plano como: (custo do novo plano proporcional aos
  dias restantes do ciclo) menos (crédito do plano antigo proporcional aos
  mesmos dias restantes), usando o número real de dias do ciclo vigente
  (ver FR-021) como duração do ciclo (não um valor fixo), e MUST expor esse
  valor calculado ao usuário antes da confirmação.
- **FR-010**: Ao confirmar um upgrade, o sistema MUST definir a data de
  término da matrícula anterior como a data do upgrade e criar uma nova
  matrícula ativa (sem data de término) no plano superior, com início na
  data do upgrade.
- **FR-011**: O sistema MUST rejeitar tentativas de upgrade para um plano de
  valor mensal igual ou inferior ao da matrícula atual (downgrade não
  permitido).
- **FR-012**: O sistema MUST permitir cancelar uma matrícula ativa, definindo
  sua data de término como o fim do ciclo mensal pago vigente (sem estorno do
  valor já pago).
- **FR-013**: O sistema MUST excluir matrículas cuja data de término seja
  anterior a hoje de qualquer cálculo de receita ativa (R4), mantendo-as
  visíveis no histórico e na listagem geral.
- **FR-014**: O sistema MUST permitir que um Pokémon com matrícula encerrada
  (data de término anterior a hoje) seja matriculado novamente em qualquer
  plano, sem tratar isso como matrícula duplicada.
- **FR-015**: O sistema MUST permitir transferir um Pokémon de um Treinador
  para outro de forma direta e imediata, sem exigir aceite do Treinador de
  destino. Se o Pokémon tiver uma matrícula ativa no momento da transferência,
  o sistema MUST (a) definir a data de término dessa matrícula como a data da
  transferência, preservando o registro no histórico associado ao Pokémon,
  sem estornar o valor já pago pelo Treinador de origem, e (b) criar
  automaticamente uma nova matrícula ativa no mesmo plano sob o Treinador de
  destino, com início na data da transferência e cobrança integral do valor
  mensal do plano (sem cálculo de pro-rata, por se tratar de um novo ciclo de
  cobrança), sem exigir nenhuma ação manual do Treinador de destino.
- **FR-016**: O sistema MUST permitir listar matrículas com busca textual por
  nome do Pokémon ou do Treinador.
- **FR-017**: O sistema MUST permitir filtrar a listagem de matrículas por
  estado derivado da data de término: Ativa (sem data de término), Ativa "a
  encerrar" (data de término igual ou posterior a hoje) ou Encerrada (data de
  término anterior a hoje).
- **FR-018**: O sistema MUST validar campos obrigatórios no formulário de
  nova matrícula (Pokémon, Plano) antes de permitir o envio; a data de início
  não é um campo do formulário, pois é definida automaticamente pelo sistema.
- **FR-019**: O sistema MUST expor uma consulta que retorne a Receita Mensal
  Recorrente agrupada por plano, considerando apenas matrículas ativas (sem
  data de término, ou com data de término igual/posterior a hoje), com uma
  linha de total geral ao final.
- **FR-020**: O sistema MUST derivar o estado de uma matrícula (Ativa, Ativa
  "a encerrar", Encerrada) exclusivamente a partir de sua data de término
  opcional, sem persistir um campo de status separado: ausência de data de
  término ou data igual/posterior a hoje MUST ser tratada como ativa; data
  anterior a hoje MUST ser tratada como encerrada.
- **FR-021**: O ciclo mensal usado no pro-rata do upgrade (FR-009) MUST ser
  definido como o intervalo entre a data de início da matrícula (ou o
  aniversário mensal mais recente dela) e a mesma data um mês depois; quando
  o dia de início não existir no mês seguinte (ex.: matrícula iniciada em 29,
  30 ou 31), o fim do ciclo MUST ser o último dia desse mês seguinte. A
  duração do ciclo usada no cálculo é, portanto, o número real de dias entre
  essas duas datas (28 a 31 dias, conforme o mês), nunca um valor fixo de 30
  — o exemplo do enunciado (dia 16 de um ciclo de 30 dias) é uma instância
  desse cálculo geral em um mês de 30 dias, não uma regra fixa.
- **FR-022**: O sistema MUST restringir o tipo do Pokémon a uma lista fixa de
  18 valores (Normal, Fogo, Água, Planta, Elétrico, Gelo, Lutador, Venenoso,
  Terra, Voador, Psíquico, Inseto, Pedra, Fantasma, Dragão, Sombrio, Aço,
  Fada), rejeitando qualquer valor fora dessa lista.

### Key Entities

- **Treinador**: pessoa que possui Pokémon e contrata planos de treinamento
  em nome deles. Atributos: nome, e-mail (único), cidade de origem.
- **Pokémon**: animal de treinamento vinculado a um Treinador dono.
  Atributos: nome, tipo (um dos 18 tipos fixos: Normal, Fogo, Água, Planta,
  Elétrico, Gelo, Lutador, Venenoso, Terra, Voador, Psíquico, Inseto, Pedra,
  Fantasma, Dragão, Sombrio, Aço, Fada), nível (1-100). Pode ser transferido
  de um Treinador para outro.
- **Plano de Treinamento**: conjunto fixo de três planos (Ginásio Local R$
  50,00; Liga Regional R$ 120,00; Elite dos 4 R$ 300,00), cada um com valor
  mensal e descrição.
- **Matrícula**: vincula um Pokémon a um Plano de Treinamento em um
  determinado intervalo de tempo. Atributos: data de início (definida
  automaticamente no momento da contratação, não editável pelo usuário),
  data de término (opcional — sua ausência, ou um valor igual/posterior a
  hoje, indica matrícula ativa; um valor anterior a hoje indica matrícula
  encerrada; não há um campo de status separado), valor mensal vigente. Um
  Pokémon pode ter várias matrículas ao longo do tempo (histórico), mas no
  máximo uma ativa por vez.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das tentativas de criar uma segunda matrícula Ativa para
  o mesmo Pokémon são rejeitadas com uma mensagem de erro compreensível para
  o usuário final (R1).
- **SC-002**: O valor da primeira cobrança calculado em um upgrade corresponde
  exatamente ao valor esperado no exemplo do enunciado (R$ 35,00 para upgrade
  de Ginásio Local para Liga Regional no dia 16 de um ciclo de 30 dias), e a
  quaisquer outras combinações de plano/dia restante/duração real do mês
  testadas, incluindo ciclos iniciados em dias 29-31 (R2).
- **SC-003**: 100% das tentativas de matricular ou fazer upgrade para o plano
  Elite dos 4 com Pokémon de nível abaixo de 50 são rejeitadas, e 100% das
  tentativas com nível 50 ou superior são aceitas (R3).
- **SC-004**: 100% das tentativas de downgrade de plano são rejeitadas com
  uma mensagem de erro compreensível.
- **SC-005**: Um usuário consegue localizar uma matrícula específica por nome
  do Pokémon, nome do Treinador, ou status, em uma única operação de busca ou
  filtro.
- **SC-006**: A consulta de MRR retorna o total mensal recorrente correto por
  plano e um total geral, refletindo exclusivamente matrículas em status
  Ativa, verificável manualmente contra uma massa de dados de teste conhecida.
- **SC-007**: Matrículas encerradas permanecem visíveis no histórico e na
  listagem, mas não influenciam o valor de nenhum relatório de receita ativa.
- **SC-008**: 100% das transferências de um Pokémon com matrícula ativa
  resultam na matrícula anterior encerrada com data de término igual à data
  da transferência (sem perda do registro histórico) e em uma nova matrícula
  ativa criada automaticamente no mesmo plano sob o Treinador de destino, sem
  exigir nenhuma ação manual adicional (R5).

## Assumptions

- **Sem autenticação/login**: o sistema é tratado como uma ferramenta interna
  de gestão do Centro de Treinamento, sem controle de acesso por usuário ou
  perfil de permissão — qualquer pessoa com acesso à aplicação pode realizar
  qualquer operação. Essa premissa deve ser registrada no README (Princípio
  III da constituição do projeto).
- **Planos de treinamento são fixos**: os três planos (Ginásio Local, Liga
  Regional, Elite dos 4) são um conjunto pré-definido conhecido em tempo de
  design; a feature não inclui uma tela de CRUD de planos.
- **Status derivado, sem campo persistido**: em vez de um enum de status
  (Ativa/Cancelada/Concluída), a matrícula usa apenas uma data de término
  opcional; o estado exibido ao usuário (Ativa, Ativa "a encerrar", Encerrada)
  é sempre calculado a partir dessa data, nunca armazenado separadamente —
  decisão registrada para resolver a ambiguidade original do enunciado sobre
  a diferença entre "Cancelada" e "Concluída" (R4).
- **Transferência de Pokémon é direta, imediata e recria a matrícula
  automaticamente**: não há fluxo de solicitação/aceite entre o Treinador de
  origem e o de destino nesta feature; a transferência se efetiva com a
  confirmação do Treinador de origem. Se o Pokémon tiver matrícula ativa, o
  sistema encerra automaticamente a matrícula sob o Treinador de origem e
  cria uma nova matrícula equivalente (mesmo plano) sob o Treinador de
  destino, cobrada integralmente a partir da data da transferência; o
  pagamento já feito pelo Treinador de origem não é estornado, e qualquer
  acerto financeiro entre os dois Treinadores é externo à plataforma (R5).
- **Duração do ciclo mensal (R2)**: o ciclo de cada matrícula termina na
  mesma data um mês após seu início; quando essa data não existir no mês
  seguinte, o ciclo termina no último dia desse mês. O exemplo do enunciado
  ("ciclo de 30 dias") é tratado como uma instância desse cálculo geral, não
  como um valor fixo.
- **Data de início da matrícula = momento da contratação**: não é um campo
  preenchido manualmente; o sistema a define automaticamente como o instante
  da confirmação da matrícula, análogo ao momento em que os dados de
  pagamento seriam capturados em um sistema real de cobrança recorrente.
- **Tipos de Pokémon são uma lista fixa de 18 valores** (os tipos oficiais da
  franquia), não um campo de texto livre.
- **Unicidade de e-mail do Treinador** é avaliada de forma case-insensitive
  (ex.: "Ana@x.com" e "ana@x.com" são tratados como o mesmo e-mail).
- **Nível do Pokémon** é sempre um número inteiro entre 1 e 100, inclusive
  nos dois extremos.
- **Planos sem matrícula ativa não aparecem como linha própria** na consulta
  de MRR (FR-019); apenas planos com pelo menos uma matrícula ativa geram
  linha, além da linha de total geral, que sempre aparece mesmo que o total
  seja zero.
