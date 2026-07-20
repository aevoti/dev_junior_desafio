# Implementation Plan: Gestão de Matrículas do Centro de Treinamento Pokémon

**Branch**: `001-enrollment-management` | **Date**: 2026-07-17 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-enrollment-management/spec.md`

## Summary

Sistema de gestão de matrículas (Treinador, Pokémon, Plano, Matrícula) com
três regras de negócio críticas — matrícula única ativa (R1), upgrade com
cobrança pro-rata (R2) e nível mínimo para Elite dos 4 (R3) — mais
cancelamento (R4) e transferência de Pokémon com recriação automática de
matrícula (R5). Abordagem técnica: API REST em ASP.NET Core (.NET 8) com
EF Core sobre SQL Server para CRUD e regras de negócio, um SQL puro dedicado
(`consulta-mrr.sql`) para o relatório de MRR, e um frontend Angular com
formulários reativos para as 3 telas mínimas exigidas. Sem autenticação, sem
dependências além do necessário para as 5 user stories do spec.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (LTS) no backend; TypeScript 5.x /
Angular (versão estável mais recente via Angular CLI, standalone components)
no frontend.

**Primary Dependencies**:
- Backend: ASP.NET Core Web API, Entity Framework Core 8 (provider SQL
  Server) para persistência e migrations, xUnit para testes.
- Frontend: Angular CLI, Angular Reactive Forms, Angular HttpClient (RxJS
  embutido), Bootstrap (apenas CSS, sem Angular Material) para formulários,
  tabelas, alertas de erro e badges de status — ver `research.md` item 6.

**Storage**: SQL Server rodando em um único container Docker, orquestrado
por `docker-compose.yml` na raiz do repositório (`docker compose up db`) —
portátil entre Windows/Mac/Linux, sem exigir instalação de SQL Server na
máquina de quem avalia (ver `research.md` item 8). Backend e frontend
rodam nativos (`dotnet run`, `ng serve`), não em container. Schema
gerenciado via EF Core Code-First; `database/schema.sql` é exportado das
migrations do EF Core (ver `research.md` item 2) para satisfazer o
entregável "script de criação" pedido pelo enunciado sem manter duas
fontes de verdade divergentes. `database/consulta-mrr.sql` é escrita à mão
em T-SQL puro, sem ORM, contra o schema gerado (exigência explícita do
enunciado e da constituição do projeto).

**Testing**: xUnit para o backend, com foco obrigatório (Princípio I da
constituição) na lógica de negócio de R1, R2 e R3, incluindo os casos de
borda listados no spec (ciclo com dias 29-31, arredondamento, downgrade,
nível limite 50). Testes de integração de API e testes de frontend
(Angular/Karma) são desejáveis mas não obrigatórios — tratados como
prioridade menor dado o orçamento de 4-8h (Princípio IV), registrados em
"Melhorias futuras" no README se não couberem no tempo disponível.

**Target Platform**: aplicação web executada localmente (backend Kestrel via
`dotnet run`, frontend via `ng serve`), sem deploy ou infraestrutura de
produção — o enunciado só exige rodar localmente com instruções claras no
README.

**Project Type**: Web application (frontend Angular + backend ASP.NET Core)
mais uma pasta `database/` para os scripts SQL — layout já fixado pela
constituição do projeto (`/backend`, `/frontend`, `/database`).

**Performance Goals**: Não especificado pelo enunciado; nenhuma meta formal
de performance é exigida — ferramenta de uso interno, sem requisito de
escala (N/A, não é critério avaliado).

**Constraints**:
- `consulta-mrr.sql` MUST ser SQL puro, sem ORM (constituição, Stack
  Técnica).
- Sem autenticação/login (Assumption do spec, Princípio III).
- Sem dependências além do necessário para as 5 user stories (Princípio IV).

**Scale/Scope**: Uso interno de um único Centro de Treinamento, volume de
dados de teste/demonstração — não é um requisito de escala do desafio.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio / Seção | Verificação | Status |
|---|---|---|
| I. Corretude das Regras de Negócio | Estrutura inclui projeto de testes xUnit dedicado com foco obrigatório em R1-R3 e seus casos de borda (ver `Technical Context > Testing`) | PASS |
| II. Convenção Bilíngue de Idioma | Entidades, colunas, código e nomes de projeto em inglês (`data-model.md`); mensagens de erro ao usuário e este plano em português | PASS |
| III. Documentação de Decisões e Premissas | R4/R5 e as 3 clarificações adicionais (arredondamento, MRR vazio, busca) já documentadas em `spec.md` Assumptions e em README.md antes deste plano | PASS |
| IV. Simplicidade e Escopo Mínimo | Stack deliberadamente enxuto: sem MediatR/AutoMapper/FluentValidation no backend, sem Angular Material no frontend (ver `research.md`) | PASS |
| V. Transparência no Uso de IA | README "Uso de IA" já documenta o fluxo Spec Kit até aqui; deve continuar sendo atualizado durante `/speckit-tasks` e `/speckit-implement` | PASS (obrigação contínua, não bloqueia este plano) |
| Stack Técnica e Estrutura do Projeto | `/backend`, `/frontend`, `/database` na raiz; `consulta-mrr.sql` em SQL puro sem ORM | PASS |
| Fluxo de Desenvolvimento e Entrega | Sequência constitution → specify → clarify → plan já seguida; README já tem as seções obrigatórias | PASS |

Nenhuma violação identificada — `Complexity Tracking` permanece vazio.

**Re-check pós Fase 1** (após `data-model.md`, `contracts/api.md` e
`quickstart.md`): nenhuma entidade, endpoint ou dependência nova introduzida
nesses artefatos contradiz a tabela acima — o índice único filtrado e o
enum `PokemonType` reforçam R1/R3 sem adicionar dependências externas, e os
dois endpoints de upgrade (preview/confirm) continuam sem bibliotecas além
das já listadas em `Technical Context`. Gate PASS mantido.

**Re-check após discussão de stack (Bootstrap + Docker)**: Bootstrap
(dependência CSS, sem módulos Angular) e Docker Compose com um único
serviço de banco foram avaliados contra o Princípio IV e considerados
compatíveis — ambos reduzem, em vez de aumentar, o tempo gasto em setup e
estilização não avaliados pelo enunciado (ver `research.md` itens 6 e 8).
Gate PASS mantido.

## Project Structure

### Documentation (this feature)

```text
specs/001-enrollment-management/
├── plan.md              # Este arquivo (/speckit-plan)
├── research.md          # Fase 0 (/speckit-plan)
├── data-model.md         # Fase 1 (/speckit-plan)
├── quickstart.md         # Fase 1 (/speckit-plan)
├── contracts/            # Fase 1 (/speckit-plan)
│   └── api.md
├── checklists/
│   └── requirements.md
└── tasks.md              # Fase 2 (/speckit-tasks)
```

### Source Code (repository root)

```text
docker-compose.yml   # Serviço único: SQL Server (backend/frontend rodam nativos)

backend/
├── src/
│   ├── PokemonTrainingCenter.Api/
│   │   ├── Controllers/       # TrainersController, PokemonsController, EnrollmentsController, TrainingPlansController
│   │   ├── Contracts/         # DTOs de request/response (ver contracts/api.md)
│   │   ├── Middleware/        # ErrorHandlingMiddleware (mensagens de erro em português)
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── PokemonTrainingCenter.Domain/
│   │   ├── Entities/          # Trainer, Pokemon, TrainingPlan, Enrollment
│   │   ├── Enums/             # PokemonType (18 valores fixos)
│   │   ├── Exceptions/        # DomainValidationException (mensagem + status code)
│   │   ├── Persistence/       # AppDbContext (movido de Infrastructure — ver nota abaixo)
│   │   └── Services/          # EnrollmentService (R1-R5), BillingCycleCalculator (R2)
│   └── PokemonTrainingCenter.Infrastructure/
│       └── Migrations/        # EF Core migrations (origem do database/schema.sql);
│                               # MigrationsAssembly configurado explicitamente no
│                               # Program.cs para apontar aqui, já que o AppDbContext
│                               # mora no assembly do Domain
└── tests/
    ├── PokemonTrainingCenter.UnitTests/     # R1-R5 e BillingCycleCalculator (obrigatório)
    └── PokemonTrainingCenter.IntegrationTests/ # Testes de API ponta a ponta (se o tempo permitir)

frontend/
└── src/
    └── app/
        ├── trainers/
        │   ├── trainer-form/       # Cadastro de Treinador (US1)
        │   └── trainers-list/      # Listagem de Treinadores, sem filtro (US6)
        ├── pokemons/
        │   ├── pokemon-form/       # Cadastro de Pokémon (US1)
        │   ├── pokemon-transfer/   # Transferência de Pokémon (US4)
        │   └── pokemons-list/      # Listagem de Pokémons, sem filtro (US6)
        ├── enrollments/            # Listagem + busca + filtro por status (US3)
        ├── enrollment-form/        # Nova matrícula (US1)
        ├── enrollment-upgrade/     # Fluxo de upgrade com preview do valor pro-rata (US2)
        ├── shared/
        │   ├── models/             # Interfaces TS espelhando os DTOs da API
        │   └── services/           # TrainerApiService, PokemonApiService, EnrollmentApiService
        └── core/
            └── error-handling/     # Interceptor HTTP → mensagens amigáveis (R1, R2, R3)

database/
├── schema.sql             # Exportado de `dotnet ef migrations script`
└── consulta-mrr.sql       # SQL puro, sem ORM
```

**Nota de implementação (correção de arquitetura)**: o desenho original
colocava `AppDbContext` em `Infrastructure`, mas isso criaria uma referência
circular assim que `EnrollmentService` (em `Domain`) precisasse consultar o
banco — `Infrastructure` já referencia `Domain` para usar as entidades. O
`AppDbContext` foi movido para `Domain/Persistence/`, que passou a
referenciar `Microsoft.EntityFrameworkCore` e `Microsoft.EntityFrameworkCore.Relational`
diretamente (pacotes agnósticos de provedor — não acoplam o Domain ao SQL
Server especificamente). As Migrations continuam fisicamente em
`Infrastructure/Migrations` (que mantém o pacote `Microsoft.EntityFrameworkCore.SqlServer`),
com o assembly de destino configurado explicitamente via
`MigrationsAssembly("PokemonTrainingCenter.Infrastructure")` no `Program.cs`.

**Structure Decision**: Web application de dois projetos (`backend/`,
`frontend/`) mais `database/` para os scripts SQL — exatamente o layout que
a constituição do projeto já fixa. Dentro do backend, três projetos .NET
(Api / Domain / Infrastructure) separam a lógica de negócio crítica
(Domain) da infraestrutura de persistência e da camada HTTP, o mínimo de
camadas necessário para testar R1-R5 isoladamente sem acoplar os testes a
ASP.NET Core ou ao EF Core.

## Complexity Tracking

> Nenhuma violação do Constitution Check — seção não aplicável a este plano.

## Nota de revisão pós-implementação (2026-07-20)

Após a primeira entrega completa (todas as tasks de `tasks.md` em `[X]`),
uma revisão manual do código identificou 5 correções técnicas necessárias,
sem alterar a arquitetura descrita acima:

- **Change detection zoneless mal configurado**: o `frontend/package.json`
  não depende de `zone.js` e `app.config.ts` não declara
  `provideZonelessChangeDetection()` — a aplicação já roda zoneless (padrão
  do Angular 22), mas sem os providers corretos, mutações de campo simples
  dentro de `.subscribe()` não disparam re-render sozinhas. Fix: declarar
  `provideZonelessChangeDetection()` explicitamente e migrar o estado de
  listas/loading (`enrollments-list`, os 3 formulários) para `signal()`, que
  o scheduler zoneless já rastreia nativamente. Explica dois sintomas
  reportados: listagem de matrículas parada até interação, e `<select>` que
  só abre as opções no segundo clique.
- **`output()` sem consumidor**: os 3 formulários (`enrollment-form`,
  `trainer-form`, `pokemon-form`) emitem `output<T>()` mas são carregados
  via rota (`loadComponent`), sem componente pai ouvindo o evento — por
  isso nenhuma mensagem de sucesso aparecia. Fix: trocar por navegação via
  `Router` para a listagem correspondente (FR-023), com um sinal de sucesso
  passado por router state.
- **Validação de e-mail permissiva**: `Validators.email` do Angular aceita
  endereços sem TLD (ex.: `nome@dominio`). Fix: regex customizada (FR-024).
- **Granularidade do status derivado por instante, não por data (FR-020)**:
  `Enrollment.IsActive`, `EnrollmentResponse.ResolveStatus` e as duas
  consultas de "matrícula ativa" em `EnrollmentService` truncavam `EndDate`
  para `.Date` antes de comparar. Fix: comparação pelo instante completo,
  com `CancelEnrollmentAsync` passando a gravar o fim do dia (UTC) do ciclo
  em vez do instante exato do ciclo (ver Assumptions do spec.md). Isso
  também elimina a necessidade do workaround `OrderByDescending(StartDate)`
  documentado em `TransferPokemonAsync` — sem truncar para `.Date`, uma
  matrícula já substituída no mesmo dia deixa de aparecer na consulta de
  "matrícula ativa" imediatamente, então a ambiguidade que motivou o
  `OrderByDescending` não existe mais.
- **`consulta-mrr.sql` usava `GETDATE()` (hora do servidor) em vez de
  `GETUTCDATE()`**: inconsistente com o backend (`DateTime.UtcNow`) e,
  combinado com a comparação por data, causava contagem duplicada de
  receita no dia de um upgrade/transferência (a matrícula antiga, já
  encerrada, ainda batia com `EndDate >= CAST(GETDATE() AS date)`). Fix:
  `GETUTCDATE()` e comparação por instante, consistente com a mudança
  acima.

Nenhuma dessas correções introduz dependência nova nem contradiz o
Constitution Check original — Gate PASS mantido.

**Achado durante a verificação ponta a ponta (não estava na lista original)**:
ao validar T074 (exibição de datas) contra a API e o banco reais, a coluna
"Início" mostrou o valor UTC cru (ex.: 14:40) em vez da hora local de
Brasília (11:40) — o mesmo sintoma do apontamento original sobre horários
"+3h", agora visível porque nenhuma tela exibia datas antes desta revisão.
Causa raiz: o SQL Server (`datetime2`) não preserva `DateTimeKind`; ao reler
do banco, o EF Core devolve `Kind=Unspecified`, e o `System.Text.Json` só
inclui o sufixo `Z` quando `Kind=Utc` — sem o `Z`, o `DatePipe` do Angular
interpreta a string como já sendo hora local do navegador, sem nenhuma
conversão. Fix: um `ValueConverter` global em `AppDbContext.ConfigureConventions`
que marca todo `DateTime`/`DateTime?` lido do banco como `DateTimeKind.Utc`,
garantindo que a API sempre serialize com `Z` e o frontend converta
corretamente. Validado visualmente contra a API e o banco reais via
Playwright: a mesma matrícula que antes mostrava "14:40" passou a mostrar
"11:40" após o fix, sem nenhuma mudança no dado armazenado.
