# Data Model: Gestão de Matrículas do Centro de Treinamento Pokémon

Nomes de entidades, campos e tabelas em inglês (Princípio II da
constituição). Tipos e constraints abaixo são o contrato conceitual — a
implementação exata (EF Core Fluent API vs. Data Annotations) fica a
critério das tarefas de implementação.

## Trainer (Treinador)

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | `int` (PK, identity) | — |
| `Name` | `nvarchar(200)` | Obrigatório |
| `Email` | `nvarchar(320)` | Obrigatório; único (case-insensitive — collation padrão do SQL Server já é case-insensitive) — FR-002 |
| `City` | `nvarchar(200)` | Obrigatório |

**Índices**: `UNIQUE INDEX UX_Trainer_Email ON Trainer(Email)`.

## Pokemon

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | `int` (PK, identity) | — |
| `Name` | `nvarchar(200)` | Obrigatório |
| `Type` | `nvarchar(20)` | Obrigatório; `CHECK` restringindo aos 18 valores fixos (FR-022); mapeado para o enum `PokemonType` em C# |
| `Level` | `int` | Obrigatório; `CHECK (Level BETWEEN 1 AND 100)` — FR-004 |
| `TrainerId` | `int` (FK → `Trainer.Id`) | Obrigatório; atualizável (transferência, R5) |

**Relacionamentos**: `Trainer` 1 — N `Pokemon`.

**Nota (R5)**: a transferência de Pokémon é um `UPDATE` de `Pokemon.TrainerId`
— não existe uma tabela de "transferências" nesta feature (fluxo de
solicitação/aceite fica registrado como melhoria futura no README).

## TrainingPlan (Plano de Treinamento)

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | `int` (PK, identity) | — |
| `Name` | `nvarchar(100)` | Obrigatório; conteúdo em português (nome comercial do plano, ex. "Ginásio Local") — dado de negócio, não identificador de código |
| `MonthlyPrice` | `decimal(10,2)` | Obrigatório |
| `Description` | `nvarchar(500)` | Obrigatório |

**Seed data** (conjunto fixo, constituição — Stack Técnica):

| Name | MonthlyPrice | Description |
|---|---|---|
| Ginásio Local | 50.00 | Treinos básicos |
| Liga Regional | 120.00 | Treinos intermediários + batalhas simuladas |
| Elite dos 4 | 300.00 | Preparação completa para a Liga |

**Regra de negócio associada**: apenas o plano "Elite dos 4" exige
`Pokemon.Level >= 50` para matrícula ou upgrade (R3, FR-007). Essa regra é
validada em código (`EnrollmentService`), não modelada como dado, já que só
existe uma exceção entre os três planos.

## Enrollment (Matrícula)

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | `int` (PK, identity) | — |
| `PokemonId` | `int` (FK → `Pokemon.Id`) | Obrigatório |
| `TrainingPlanId` | `int` (FK → `TrainingPlan.Id`) | Obrigatório |
| `TrainerId` | `int` (FK → `Trainer.Id`) | Obrigatório; snapshot do dono do Pokémon no momento da criação da matrícula — permanece inalterado mesmo que o Pokémon seja transferido depois (diferente de `Pokemon.TrainerId`, que sempre reflete o dono atual). Ver FR-027, Session 2026-07-20 (2). |
| `StartDate` | `datetime2` | Obrigatório; definida pelo sistema no momento da criação (FR-005) — não editável via API |
| `EndDate` | `datetime2`, nullable | Ausente ou instante `>= agora` ⇒ ativa; instante `< agora` ⇒ encerrada (FR-020) |
| `MonthlyPrice` | `decimal(10,2)` | Snapshot do `TrainingPlan.MonthlyPrice` no momento da criação da matrícula (planos são fixos, mas o snapshot evita qualquer acoplamento a mudanças futuras de preço) |

**Relacionamentos**: `Pokemon` 1 — N `Enrollment`; `TrainingPlan` 1 — N
`Enrollment`; `Trainer` 1 — N `Enrollment` (snapshot histórico do dono no
momento da criação — distinto da relação `Trainer` 1 — N `Pokemon`, que
reflete o dono atual).

**Estado derivado** (não persistido, calculado em cada leitura pelo
instante exato — FR-020; ver Session 2026-07-20 (1) para a distinção entre
cancelamento e upgrade/transferência):

| Condição sobre `EndDate` | Estado exibido |
|---|---|
| `NULL` | Ativa |
| instante `>= agora` | Ativa "a encerrar" |
| instante `< agora` | Encerrada |

**Índices e constraints**:
- `INDEX IX_Enrollment_PokemonId ON Enrollment(PokemonId)`.
- `UNIQUE INDEX UX_Enrollment_PokemonId_Open ON Enrollment(PokemonId) WHERE EndDate IS NULL`
  — defesa em profundidade para R1 sob concorrência (ver `research.md`
  item 5); complementa (não substitui) a validação em nível de aplicação
  que também cobre o caso de `EndDate >= hoje`.

**Transições de estado** (nenhuma é reversível — sempre cria-se uma nova
linha em vez de "reabrir" uma matrícula encerrada):

```text
[criação]         → Enrollment(EndDate = NULL, TrainerId = dono atual do Pokémon)          [FR-005]
Ativa → Cancelada  → EndDate = fim do dia (UTC) do ciclo pago vigente, TrainerId inalterado [FR-012]
Ativa → Upgrade    → EndDate = instante do upgrade + nova Enrollment
                       (TrainerId igual ao da matrícula anterior)                          [FR-010]
Ativa → Transferida → EndDate = instante da transferência (TrainerId da matrícula fechada
                       permanece o Treinador de origem) + nova Enrollment sob o novo
                       Trainer (TrainerId = novo Treinador), mesmo TrainingPlanId           [FR-015]
```

## PokemonType (enum, não é tabela)

Lista fixa de 18 valores (FR-022), modelada como `enum PokemonType` em C#
e como `CHECK` constraint em `Pokemon.Type` no banco (ver `research.md` —
optou-se por não criar uma tabela de lookup para evitar um JOIN
desnecessário em um conjunto de valores que nunca muda):

`Normal, Fire, Water, Grass, Electric, Ice, Fighting, Poison, Ground,
Flying, Psychic, Bug, Rock, Ghost, Dragon, Dark, Steel, Fairy`

(nomes do enum em inglês — Princípio II; a tradução para os rótulos em
português exibidos no frontend — Fogo, Água, Planta, Elétrico, Gelo,
Lutador, Venenoso, Terra, Voador, Psíquico, Inseto, Pedra, Fantasma, Dragão,
Sombrio, Aço, Fada — é responsabilidade da camada de apresentação, não do
modelo de dados).
