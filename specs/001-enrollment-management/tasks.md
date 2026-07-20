---
description: "Task list for Gestão de Matrículas do Centro de Treinamento Pokémon"
---

# Tasks: Gestão de Matrículas do Centro de Treinamento Pokémon

**Input**: Design documents from `specs/001-enrollment-management/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/api.md, research.md, quickstart.md

**Tests**: por padrão, tarefas de teste são opcionais no Spec Kit — mas a
constituição do projeto (Princípio I, NÃO NEGOCIÁVEL) torna os testes
automatizados de R1, R2 e R3 **obrigatórios**. Por isso as fases de User
Story 1 e User Story 2 têm uma subseção "Tests (OBRIGATÓRIO)". Testes de
R4/R5 (User Story 4) e de busca (User Story 3) são recomendados, mas não
bloqueiam a entrega.

**Organization**: tarefas agrupadas por user story (spec.md), na ordem de
prioridade P1 → P5, para permitir implementação e teste incremental.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: pode rodar em paralelo (arquivos diferentes, sem dependência de tarefa incompleta)
- **[Story]**: a qual user story a tarefa pertence (US1-US5)
- Caminhos de arquivo exatos em cada descrição

## Path Conventions

Layout fixado em `plan.md` (Web application):
- `backend/src/PokemonTrainingCenter.Api/` — Controllers, DTOs, Program.cs
- `backend/src/PokemonTrainingCenter.Domain/` — Entities, Enums, Services
- `backend/src/PokemonTrainingCenter.Infrastructure/` — DbContext, Migrations
- `backend/tests/PokemonTrainingCenter.UnitTests/`
- `frontend/src/app/`
- `database/` — `schema.sql`, `consulta-mrr.sql`
- `docker-compose.yml` — raiz do repositório

---

## Phase 1: Setup

**Purpose**: inicialização dos projetos e ferramentas, sem lógica de negócio ainda.

- [X] T001 Criar a estrutura de diretórios raiz: `backend/src/`, `backend/tests/`, `frontend/`, `database/` conforme `plan.md` (Project Structure)
- [X] T002 Inicializar a solution .NET e os 5 projetos (`PokemonTrainingCenter.Api`, `.Domain`, `.Infrastructure`, `.UnitTests`, `.IntegrationTests`) com referências entre projetos e pacotes NuGet (`Microsoft.EntityFrameworkCore.SqlServer`, `xunit`) em `backend/` — `.IntegrationTests` é opcional/stretch (plan.md Technical Context > Testing); só popule com testes reais se sobrar tempo após a Fase 7
- [X] T003 [P] Inicializar o workspace Angular (standalone components) em `frontend/`
- [X] T004 [P] Adicionar Bootstrap ao frontend (`npm install bootstrap`; importar em `frontend/src/styles.css` ou `angular.json`) — research.md item 6
- [X] T005 [P] Criar `docker-compose.yml` na raiz com um único serviço de SQL Server — research.md item 8
- [X] T006 [P] Configurar a connection string em `backend/src/PokemonTrainingCenter.Api/appsettings.json` apontando para o container do `docker-compose.yml`

**Checkpoint**: projetos criados e compilando vazios; `docker compose up -d db` sobe o banco.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: modelo de dados, persistência e infraestrutura compartilhada por todas as user stories.

**⚠️ CRITICAL**: nenhuma user story pode começar antes desta fase estar completa.

- [X] T007 [P] Criar o enum `PokemonType` (18 valores fixos — FR-022) em `backend/src/PokemonTrainingCenter.Domain/Enums/PokemonType.cs`
- [X] T008 [P] Criar a entidade `Trainer` (FR-001) em `backend/src/PokemonTrainingCenter.Domain/Entities/Trainer.cs`
- [X] T009 [P] Criar a entidade `Pokemon` (FR-003) em `backend/src/PokemonTrainingCenter.Domain/Entities/Pokemon.cs`
- [X] T010 [P] Criar a entidade `TrainingPlan` em `backend/src/PokemonTrainingCenter.Domain/Entities/TrainingPlan.cs`
- [X] T011 [P] Criar a entidade `Enrollment` (data-model.md) em `backend/src/PokemonTrainingCenter.Domain/Entities/Enrollment.cs`
- [X] T012 Criar `AppDbContext` com Fluent API: índice único `Trainer.Email`, `CHECK` em `Pokemon.Level` (1-100) e `Pokemon.Type` (lista fixa), índice único filtrado `Enrollment(PokemonId) WHERE EndDate IS NULL` (research.md item 5) em `backend/src/PokemonTrainingCenter.Infrastructure/Persistence/AppDbContext.cs` (depende de T007-T011)
- [X] T013 Popular os 3 `TrainingPlan` fixos via `HasData` (Ginásio Local R$50, Liga Regional R$120, Elite dos 4 R$300 — data-model.md) em `backend/src/PokemonTrainingCenter.Infrastructure/Persistence/AppDbContext.cs` (depende de T012)
- [X] T014 Gerar a migration inicial do EF Core e exportar `database/schema.sql` via `dotnet ef migrations script` (research.md item 2) (depende de T013)
- [X] T015 Configurar `Program.cs` (injeção de dependência, `AppDbContext`, CORS para o dev server do Angular, controllers) em `backend/src/PokemonTrainingCenter.Api/Program.cs` (depende de T012)
- [X] T016 [P] Criar middleware global de tratamento de exceções retornando `{ "message": "..." }` em português (contracts/api.md) em `backend/src/PokemonTrainingCenter.Api/Middleware/ErrorHandlingMiddleware.cs`
- [X] T017 [P] Implementar `TrainingPlansController` (`GET /api/training-plans`) em `backend/src/PokemonTrainingCenter.Api/Controllers/TrainingPlansController.cs` (depende de T013, T015)
- [X] T018 [P] Criar os modelos TypeScript compartilhados (`Trainer`, `Pokemon`, `TrainingPlan`, `Enrollment`) em `frontend/src/app/shared/models/`
- [X] T019 [P] Criar interceptor HTTP que traduz respostas de erro da API em mensagens amigáveis em `frontend/src/app/core/error-handling/error.interceptor.ts`
- [X] T020 [P] Configurar o esqueleto de rotas do Angular em `frontend/src/app/app.routes.ts`

**Checkpoint**: banco criado com schema e seed; `GET /api/training-plans` retorna os 3 planos; frontend com modelos e interceptor prontos para as telas.

---

## Phase 3: User Story 1 - Matricular Pokémon em Plano de Treinamento (Priority: P1) 🎯 MVP

**Goal**: um Treinador cadastra um Pokémon e o matricula em um plano, com R1 (matrícula única ativa) e R3 (nível mínimo Elite dos 4) validados.

**Independent Test**: cadastrar Treinador + Pokémon, matricular em um plano, confirmar status ativo e valor correto; repetir a matrícula deve ser rejeitada (R1); tentar Elite dos 4 com nível <50 deve ser rejeitado (R3).

### Tests for User Story 1 (OBRIGATÓRIO — Princípio I da constituição) ⚠️

- [X] T021 [P] [US1] Testes unitários de `EnrollmentService.CreateEnrollment` cobrindo: R1 (matrícula duplicada rejeitada, incluindo o caso de borda em que `EndDate` é igual a hoje — ainda deve contar como ativa, spec.md Edge Cases); R3 (nível <50 rejeitado, nível=50 aceito, planos não-Elite aceitam qualquer nível); e FR-014 (matrícula com `EndDate` no passado não bloqueia nova matrícula) em `backend/tests/PokemonTrainingCenter.UnitTests/EnrollmentServiceTests.cs` — **nota de implementação**: FR-004 (nível 1-100) foi movido para a validação de criação do Pokémon (T023/PokemonsController + `CK_Pokemon_Level` no schema), já que não é uma regra do `EnrollmentService`

### Implementation for User Story 1

- [X] T022 [P] [US1] Implementar `TrainersController` (`POST`, `GET /api/trainers`) — FR-001, FR-002 em `backend/src/PokemonTrainingCenter.Api/Controllers/TrainersController.cs` (depende de T015)
- [X] T023 [P] [US1] Implementar `PokemonsController` (`POST`, `GET /api/pokemons`) — FR-003, FR-004, FR-022 em `backend/src/PokemonTrainingCenter.Api/Controllers/PokemonsController.cs` (depende de T015)
- [X] T024 [US1] Implementar `EnrollmentService.CreateEnrollment` — FR-005, FR-006, FR-007 em `backend/src/PokemonTrainingCenter.Domain/Services/EnrollmentService.cs` (depende de T008-T011)
- [X] T025 [US1] Implementar `EnrollmentsController` (`POST /api/enrollments`) em `backend/src/PokemonTrainingCenter.Api/Controllers/EnrollmentsController.cs` (depende de T024)
- [X] T026 [P] [US1] Criar `TrainerApiService` em `frontend/src/app/shared/services/trainer-api.service.ts` (depende de T018)
- [X] T027 [P] [US1] Criar `PokemonApiService` em `frontend/src/app/shared/services/pokemon-api.service.ts` (depende de T018)
- [X] T028 [P] [US1] Criar `TrainingPlanApiService` em `frontend/src/app/shared/services/training-plan-api.service.ts` (depende de T018)
- [X] T029 [P] [US1] Criar `EnrollmentApiService` (criar/listar) em `frontend/src/app/shared/services/enrollment-api.service.ts` (depende de T018)
- [X] T030 [US1] Criar o componente de cadastro de Treinador em `frontend/src/app/trainers/trainer-form/trainer-form.ts` (depende de T026) — nomenclatura de arquivo sem sufixo `.component` (convenção do Angular CLI 22, `ng generate` atual)
- [X] T031 [US1] Criar o componente de cadastro de Pokémon com validação de nível 1-100 e tipo fixo em `frontend/src/app/pokemons/pokemon-form/pokemon-form.ts` (depende de T027, T028)
- [X] T032 [US1] Criar o componente de nova matrícula (seleção de Pokémon + Plano, validações de campos obrigatórios e nível mínimo Elite dos 4 — FR-018) em `frontend/src/app/enrollment-form/enrollment-form.ts` (depende de T029)
- [X] T033 [US1] Conectar a exibição amigável (alerta Bootstrap) dos erros de R1/R3 em `frontend/src/app/enrollment-form/enrollment-form.ts` (depende de T032, T019)

**Checkpoint**: User Story 1 completa e testável de ponta a ponta — MVP alcançado.

---

## Phase 4: User Story 2 - Upgrade de Matrícula com Cobrança Pro-Rata (Priority: P2)

**Goal**: Treinador solicita upgrade de plano, vê o valor da primeira cobrança calculado antes de confirmar (R2).

**Independent Test**: com uma matrícula ativa criada na US1, solicitar upgrade em uma data conhecida do ciclo e conferir se o valor previsto bate com o esperado antes de qualquer confirmação.

### Tests for User Story 2 (OBRIGATÓRIO — Princípio I da constituição) ⚠️

- [X] T034 [P] [US2] Testes unitários de `BillingCycleCalculator` cobrindo: o exemplo do enunciado (dia 16 de um ciclo de 30 dias → R$ 35,00); upgrade no primeiro dia do ciclo (dias restantes = ciclo inteiro) e no último dia do ciclo (dias restantes ≈ 0); ciclos iniciados nos dias 29-31; e arredondamento (0,5 para cima) — spec.md SC-002 e Edge Cases; constituição Princípio I ("ciclo com 0 ou 30 dias restantes, upgrade no mesmo dia") em `backend/tests/PokemonTrainingCenter.UnitTests/BillingCycleCalculatorTests.cs`
- [X] T035 [P] [US2] Testes unitários de upgrade em `EnrollmentService` cobrindo: downgrade rejeitado (FR-011); nível mínimo Elite dos 4 em upgrade (R3); e a transição de `ConfirmUpgrade` (FR-010) — matrícula antiga com `EndDate` = data do upgrade, nova matrícula ativa criada no plano correto — em `backend/tests/PokemonTrainingCenter.UnitTests/EnrollmentServiceTests.cs`

### Implementation for User Story 2

- [X] T036 [US2] Implementar `BillingCycleCalculator` (ciclo via `DateTime.AddMonths` com clamp de fim de mês, arredondamento para 2 casas — FR-021, FR-009) em `backend/src/PokemonTrainingCenter.Domain/Services/BillingCycleCalculator.cs`
- [X] T037 [US2] Implementar `EnrollmentService.PreviewUpgrade` — FR-009, FR-011 em `backend/src/PokemonTrainingCenter.Domain/Services/EnrollmentService.cs` (depende de T036, T024)
- [X] T038 [US2] Implementar `EnrollmentService.ConfirmUpgrade` — FR-010 em `backend/src/PokemonTrainingCenter.Domain/Services/EnrollmentService.cs` (depende de T037)
- [X] T039 [US2] Implementar `EnrollmentsController` (`POST /upgrade/preview`, `POST /upgrade/confirm`) em `backend/src/PokemonTrainingCenter.Api/Controllers/EnrollmentsController.cs` (depende de T038)
- [X] T040 [US2] Criar o componente de upgrade com preview do valor antes de confirmar (US2, cenário 1) em `frontend/src/app/enrollment-upgrade/enrollment-upgrade.ts` (depende de T029)
- [X] T041 [US2] Conectar a confirmação do upgrade e o erro amigável de downgrade/nível mínimo em `frontend/src/app/enrollment-upgrade/enrollment-upgrade.ts` (depende de T040, T019, T039)

**Checkpoint**: US1 + US2 funcionando juntas — fluxo de upgrade completo com pro-rata.

---

## Phase 5: User Story 3 - Consultar e Filtrar Matrículas (Priority: P3)

**Goal**: listar matrículas com busca por nome (Pokémon/Treinador) e filtro por status.

**Independent Test**: popular matrículas em status variados e conferir se busca (inclusive sem acento/maiúscula) e filtro por status retornam os subconjuntos corretos.

### Implementation for User Story 3

*(sem testes obrigatórios pela constituição — cobertura recomendada em T058, Fase de Polish)*

- [X] T042 [US3] Adicionar o campo calculado `status` (`Active`/`EndingSoon`/`Ended` — FR-020) ao DTO de resposta de Enrollment em `backend/src/PokemonTrainingCenter.Api/Contracts/EnrollmentResponse.cs`
- [X] T043 [US3] Implementar `EnrollmentsController` (`GET /api/enrollments?search=&status=`) com busca case/accent-insensitive e filtro por status — FR-016, FR-017 em `backend/src/PokemonTrainingCenter.Api/Controllers/EnrollmentsController.cs` (depende de T025, T042) — busca normalizada em memória (remoção de diacríticos), não traduzida para SQL, para funcionar de forma idêntica em qualquer provider do EF Core
- [X] T044 [US3] Criar o componente de listagem de matrículas (tabela Bootstrap, busca, filtro por status, com os códigos de status da API traduzidos para os rótulos em português — `Active`→"Ativa", `EndingSoon`→"A encerrar", `Ended`→"Encerrada") em `frontend/src/app/enrollments/enrollments-list/enrollments-list.ts` (depende de T029)
- [X] T045 [US3] Adicionar mensagem de "nenhum resultado" para busca vazia (US3, cenário 4) em `frontend/src/app/enrollments/enrollments-list/enrollments-list.ts` (depende de T044)

**Checkpoint**: as 3 telas mínimas exigidas pelo enunciado já existem e funcionam (listagem, formulário, upgrade).

---

## Phase 6: User Story 4 - Cancelar Matrícula e Transferir Pokémon entre Treinadores (Priority: P4)

**Goal**: cancelar matrícula (R4) e transferir Pokémon entre Treinadores com recriação automática de matrícula (R5).

**Independent Test**: cancelar uma matrícula ativa e conferir que ela some da receita ativa; transferir um Pokémon com matrícula ativa e conferir que a matrícula antiga encerra e uma nova é criada automaticamente sob o novo Treinador.

### Tests for User Story 4 (recomendado)

- [X] T046 [P] [US4] Testes unitários de cancelamento (R4) e de transferência com recriação automática, incluindo Pokémon sem matrícula ativa (R5) em `backend/tests/PokemonTrainingCenter.UnitTests/EnrollmentServiceTests.cs`

### Implementation for User Story 4

- [X] T047 [US4] Implementar `EnrollmentService.CancelEnrollment` — FR-012 em `backend/src/PokemonTrainingCenter.Domain/Services/EnrollmentService.cs` (depende de T024, T036)
- [X] T048 [US4] Implementar `EnrollmentsController` (`POST /api/enrollments/{id}/cancel`) em `backend/src/PokemonTrainingCenter.Api/Controllers/EnrollmentsController.cs` (depende de T047)
- [X] T049 [US4] Implementar `EnrollmentService.TransferPokemon` — FR-015 em `backend/src/PokemonTrainingCenter.Domain/Services/EnrollmentService.cs` (depende de T024)
- [X] T050 [US4] Implementar `PokemonsController` (`POST /api/pokemons/{id}/transfer`) em `backend/src/PokemonTrainingCenter.Api/Controllers/PokemonsController.cs` (depende de T049)
- [X] T051 [US4] Adicionar ação de cancelamento (com aviso de "sem estorno") na listagem de matrículas em `frontend/src/app/enrollments/enrollments-list/enrollments-list.ts` (depende de T044, T048)
- [X] T052 [US4] Criar o componente de transferência de Pokémon (seleção de novo Treinador) em `frontend/src/app/pokemons/pokemon-transfer/pokemon-transfer.ts` (depende de T050, T026)

**Checkpoint**: todas as regras de negócio (R1-R5) implementadas e utilizáveis pela UI.

---

## Phase 7: User Story 5 - Relatório de Receita Mensal Recorrente (MRR) por Plano (Priority: P5)

**Goal**: consulta SQL pura com o MRR agrupado por plano e uma linha de Total Geral.

**Independent Test**: rodar `database/consulta-mrr.sql` contra os dados criados nas fases anteriores e conferir que a soma por plano considera só matrículas ativas, e que a linha de Total Geral aparece mesmo com R$ 0,00.

- [X] T053 [US5] Escrever `database/consulta-mrr.sql` em T-SQL puro, sem ORM, usando `GROUPING SETS` (ou `UNION ALL`) para a linha de Total Geral sempre presente — FR-019, research.md item 3; validar manualmente contra o cenário 9 de `quickstart.md` — **validado de ponta a ponta contra o container real** (banco vazio → só Total Geral R$0; dados seed → Ginásio Local R$50 + Elite dos 4 R$600 + Total R$650, com a matrícula encerrada corretamente excluída; índice único filtrado confirmado rejeitando uma segunda matrícula aberta para o mesmo Pokémon)

**Checkpoint**: os dois entregáveis SQL do desafio (`schema.sql`, `consulta-mrr.sql`) existem e foram validados manualmente.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: melhorias que afetam múltiplas user stories, e as obrigações contínuas de documentação da constituição.

- [X] T054 [P] Adicionar comentários de documentação (XML doc, em inglês — Princípio II) aos métodos públicos de `EnrollmentService` e `BillingCycleCalculator` em `backend/src/PokemonTrainingCenter.Domain/Services/` — corrigidos 4 docs que eu tinha escrito em português por engano
- [X] T055 [P] Revisar a clareza em português de todas as mensagens de erro voltadas ao usuário final nos Controllers em `backend/src/PokemonTrainingCenter.Api/Controllers/`
- [X] T056 [P] Rodar manualmente os 9 cenários de `quickstart.md` e registrar o resultado — **rodados de ponta a ponta contra a API + banco reais** (não só testes automatizados). Cenários 1-7 e 9 passaram de primeira. O cenário 8 (transferência) revelou um **bug real**: `TransferPokemonAsync` buscava "a matrícula ativa do Pokémon" sem ordenar por `StartDate`, então no mesmo dia de um upgrade anterior podia pegar a matrícula antiga (já superada, mas ainda "ativa" pela regra de granularidade por data do FR-020) em vez da nova — violando o índice único ao tentar reabri-la. Corrigido com `OrderByDescending(StartDate)`; teste de regressão adicionado; revalidado com sucesso contra a API real
- [X] T057 Atualizar `README.md` (Instruções de execução, "O que precisei corrigir/reescrever", Melhorias futuras — Princípios III e V) com o que realmente aconteceu durante a implementação
- [X] T058 [P] Testes de busca (case/accent-insensitive) e filtro por status para `EnrollmentsController.GET` (FR-016, FR-017, US3) em `backend/tests/PokemonTrainingCenter.UnitTests/EnrollmentQueryTests.cs`

---

## Phase 9: Correções e Melhorias Pós-Revisão (2026-07-20)

**Purpose**: revisão manual do código entregue nas Fases 1-8 identificou
bugs e lacunas de UX antes da entrega final. Ver `plan.md` ("Nota de revisão
pós-implementação") para o diagnóstico técnico completo de cada item.

### Backend

- [ ] T059 [P] Corrigir a granularidade do status derivado para instante
  exato (FR-020) em `Enrollment.IsActive`
  (`backend/src/PokemonTrainingCenter.Domain/Entities/Enrollment.cs`),
  `EnrollmentResponse.ResolveStatus`
  (`backend/src/PokemonTrainingCenter.Api/Contracts/EnrollmentResponse.cs`),
  e nas duas consultas de "matrícula ativa" em
  `backend/src/PokemonTrainingCenter.Domain/Services/EnrollmentService.cs`
  (`EnsureNoActiveEnrollmentAsync`, `TransferPokemonAsync`) — trocar
  `EndDate.Value.Date >= DateTime.UtcNow.Date` por `EndDate >=
  DateTime.UtcNow`
- [ ] T060 [US4] Atualizar `CancelEnrollmentAsync` (FR-012) para gravar
  `EndDate` como o fim do dia (UTC) do ciclo pago vigente, não o instante
  exato do ciclo, em
  `backend/src/PokemonTrainingCenter.Domain/Services/EnrollmentService.cs`
  (depende de T059)
- [ ] T061 [P] Simplificar/remover o workaround `OrderByDescending(StartDate)`
  e seu comentário em `TransferPokemonAsync`
  (`backend/src/PokemonTrainingCenter.Domain/Services/EnrollmentService.cs`)
  — a ambiguidade que o motivava deixa de existir após T059 (depende de T059)
- [ ] T062 [P] Adicionar `TrainerName` denormalizado a `PokemonResponse`
  (contracts/api.md, Session 2026-07-20) em
  `backend/src/PokemonTrainingCenter.Api/Contracts/PokemonResponse.cs` e no
  `PokemonsController` (`GET`/`POST`)
- [ ] T063 [P] Adicionar validação de formato de e-mail (FR-024) no backend
  — rejeitar e-mail sem TLD de 2+ caracteres — em
  `backend/src/PokemonTrainingCenter.Api/Controllers/TrainersController.cs`
  (defesa em profundidade; a validação client-side de T068 pode ser
  contornada)
- [ ] T064 [P] [US5] Corrigir `database/consulta-mrr.sql` para usar
  `GETUTCDATE()` (em vez de `GETDATE()`) e comparação por instante,
  consistente com T059 — corrige contagem duplicada de receita no dia de um
  upgrade/transferência

### Frontend

- [ ] T065 [P] Declarar `provideZonelessChangeDetection()` explicitamente em
  `frontend/src/app/app.config.ts`
- [ ] T066 [US3] Migrar o estado de `enrollments-list` (`enrollments`,
  `loading`) para `signal()` em
  `frontend/src/app/enrollments/enrollments-list/enrollments-list.ts` —
  corrige a listagem não aparecer até interação (depende de T065)
- [ ] T067 [P] Criar um validador de e-mail customizado (exige TLD de 2+
  caracteres — FR-024) e usá-lo em `trainer-form` em
  `frontend/src/app/trainers/trainer-form/trainer-form.ts`
- [ ] T068 [US6] Criar o componente de listagem de Treinadores (sem filtro —
  FR-025) em `frontend/src/app/trainers/trainers-list/trainers-list.ts`
  (depende de T065)
- [ ] T069 [US6] Criar o componente de listagem de Pokémons (sem filtro,
  exibindo `trainerName` — FR-026) em
  `frontend/src/app/pokemons/pokemons-list/pokemons-list.ts` (depende de
  T062, T065)
- [ ] T070 [US6] Atualizar `frontend/src/app/app.routes.ts` com as rotas das
  2 novas listagens (`/treinadores`, `/pokemons`) e mover as rotas de
  formulário para caminhos filhos (`/treinadores/novo`, `/pokemons/novo`
  já existem — sem mudança de path, só de origem de navegação)
- [ ] T071 [US6] Trocar `output()` por navegação via `Router` para a
  listagem correspondente ao concluir com sucesso (FR-023), com mensagem de
  confirmação exibida na listagem via router state, nos 3 formulários:
  `frontend/src/app/enrollment-form/enrollment-form.ts`,
  `frontend/src/app/trainers/trainer-form/trainer-form.ts`,
  `frontend/src/app/pokemons/pokemon-form/pokemon-form.ts` (depende de
  T068, T069)
- [ ] T072 [US6] Exibir o alerta de sucesso vindo do router state nas 3
  listagens (`enrollments-list`, `trainers-list`, `pokemons-list`) (depende
  de T071)
- [ ] T073 [US6] Reorganizar o menu superior em `frontend/src/app/app.html`
  para linkar as 3 listagens (Matrículas, Treinadores, Pokémons); cada
  listagem ganha um botão "+ Novo" que leva ao formulário correspondente
  (depende de T068, T069)
- [ ] T074 [P] Exibir `startDate`/`endDate` convertidos para o fuso horário
  local do navegador (não UTC cru) na listagem de matrículas — nenhuma tela
  hoje exibe essas datas — em
  `frontend/src/app/enrollments/enrollments-list/enrollments-list.html`

**Checkpoint**: todos os apontamentos da revisão de 2026-07-20 corrigidos;
`dotnet test` e os cenários manuais relevantes de `quickstart.md`
revalidados; README atualizado por último (fora desta fase, ver
instruções do usuário).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Fase 1)**: sem dependências — pode começar imediatamente.
- **Foundational (Fase 2)**: depende da conclusão do Setup — bloqueia todas as user stories.
- **User Stories (Fase 3+)**: todas dependem da Fase 2 completa.
  - US1 (P1) não depende de nenhuma outra story.
  - US2 (P2) depende de `EnrollmentService.CreateEnrollment` (T024, de US1) já existir.
  - US3 (P3) depende de `EnrollmentsController` (T025, de US1) já existir.
  - US4 (P4) depende de T024 (US1) e T036 (US2, para calcular o fim do ciclo no cancelamento).
  - US5 (P5) é independente de código — só precisa do schema (T014) existir.
- **Polish (Fase 8)**: depende de todas as user stories desejadas estarem completas.

### Within Each User Story

- Testes (quando obrigatórios) antes da implementação correspondente.
- Entidades/serviços de domínio antes de controllers.
- Controllers antes dos serviços Angular que os consomem.
- Serviços Angular antes dos componentes que os usam.

### Parallel Opportunities

- Todas as tarefas `[P]` do Setup (T003-T006) em paralelo.
- Todas as entidades de domínio do Foundational (T007-T011) em paralelo.
- Dentro de cada user story, as tarefas `[P]` marcadas (ex.: T022+T023, T026-T029) em paralelo entre si.
- US3 e US4 podem ser desenvolvidas em paralelo por pessoas diferentes depois que US1/US2 estiverem prontas, já que tocam arquivos distintos (exceto ambas mexerem em `enrollments-list.component.ts` — nesse caso, sequenciar).

---

## Parallel Example: Foundational

```bash
# Entidades de domínio, todas em arquivos diferentes:
Task: "Criar a entidade Trainer em backend/src/PokemonTrainingCenter.Domain/Entities/Trainer.cs"
Task: "Criar a entidade Pokemon em backend/src/PokemonTrainingCenter.Domain/Entities/Pokemon.cs"
Task: "Criar a entidade TrainingPlan em backend/src/PokemonTrainingCenter.Domain/Entities/TrainingPlan.cs"
Task: "Criar a entidade Enrollment em backend/src/PokemonTrainingCenter.Domain/Entities/Enrollment.cs"
```

## Parallel Example: User Story 1

```bash
# Serviços Angular, todos em arquivos diferentes:
Task: "Criar TrainerApiService em frontend/src/app/shared/services/trainer-api.service.ts"
Task: "Criar PokemonApiService em frontend/src/app/shared/services/pokemon-api.service.ts"
Task: "Criar TrainingPlanApiService em frontend/src/app/shared/services/training-plan-api.service.ts"
Task: "Criar EnrollmentApiService em frontend/src/app/shared/services/enrollment-api.service.ts"
```

---

## Implementation Strategy

### MVP primeiro (User Story 1 apenas)

1. Completar Fase 1 (Setup) e Fase 2 (Foundational).
2. Completar Fase 3 (US1).
3. **Parar e validar**: rodar os testes de T021 e o cenário 1 do `quickstart.md`.
4. Nesse ponto já existe um sistema demonstrável: cadastro + matrícula com R1/R3 funcionando.

### Entrega incremental

1. Setup + Foundational → base pronta.
2. US1 → MVP demonstrável.
3. US2 → upgrade com pro-rata (a regra de maior peso na avaliação).
4. US3 → as 3 telas mínimas do enunciado completas.
5. US4 → R4/R5 completos.
6. US5 → entregável SQL de MRR.
7. Polish → documentação e revisão final antes do Pull Request.

Dado o orçamento de 4-8h (constituição, Princípio IV), se o tempo apertar,
a ordem acima já é a de prioridade: R1-R3 (US1+US2) vêm antes de R4/R5
(US4) e antes de polimento (Fase 8).
