# Centro de Treinamento Pokémon "Alto Nível"

Sistema de gestão de matrículas do desafio técnico da Aevo (vaga Desenvolvedor(a) Junior). API em .NET 8, frontend em Angular 17 e SQL Server puro, com as 5 regras de negócio (R1-R5) implementadas e testadas de ponta a ponta contra um banco real.

## Organização do repositório

```
dev_junior_desafio/
├── api/                 <- backend (.NET 8 / ASP.NET Core Web API)
├── web/                 <- frontend (Angular 17, standalone components)
├── db/                  <- scripts SQL puro (schema.sql, seed.sql, consulta-mrr.sql)
├── docker-compose.yml   <- sobe SQL Server + API (+ web, opcional)
├── setup.sh             <- automatiza o setup inicial (ver abaixo)
└── README.md
```

### `api/` — Backend (.NET 8 / ASP.NET Core Web API)

Arquitetura em camadas (Clean Architecture simplificada):

```
api/
├── src/
│   ├── PokemonTrainingCenter.Api/              <- Controllers, Program.cs, Swagger
│   ├── PokemonTrainingCenter.Domain/           <- Entidades, enums, DomainException
│   ├── PokemonTrainingCenter.Application/      <- DTOs, interfaces, MatriculaService (regras de negócio)
│   └── PokemonTrainingCenter.Infrastructure/   <- AppDbContext (EF Core), repositórios
└── tests/
    └── PokemonTrainingCenter.Tests/            <- 15 testes de unidade do MatriculaService (xUnit)
```

**Tecnologias:** .NET 8, ASP.NET Core Web API, Entity Framework Core (SQL Server), Swashbuckle (Swagger), xUnit.

### `web/` — Frontend (Angular)

```
web/src/app/
├── core/
│   ├── models/         <- interfaces TypeScript espelhando os DTOs da API
│   ├── services/        <- MatriculaService, PokemonService, PlanoTreinamentoService, ThemeService
│   └── interceptors/    <- errorInterceptor (normaliza erros da API em mensagens amigáveis)
├── features/matriculas/
│   ├── pages/matriculas-list/     <- Listagem: busca + filtro por status + ações (Upgrade/Cancelar)
│   ├── pages/matricula-form/      <- Formulário de nova matrícula (select de Pokémon, validações)
│   └── components/upgrade-modal/  <- Fluxo de upgrade: simula e mostra o valor antes de confirmar
└── shared/components/
    ├── status-badge/    <- badge colorido por status (Ativa/Cancelada/Concluída)
    └── theme-toggle/    <- botão sol/lua (tema claro/escuro)
```

**Tecnologias:** Angular 17 (standalone components, sem NgModule), TypeScript, RxJS, `@angular/animations` (transição de rota).

### `db/` — SQL Server

- `schema.sql`: DDL das 4 tabelas, com o índice único filtrado que reforça a R1 (`WHERE Status = 'Ativa'`) também a nível de banco.
- `seed.sql`: popula 10 Treinadores, 10 Pokémon e 10 Matrículas de exemplo (cobrindo os 3 planos, os 3 status e uma cadeia de upgrade). Idempotente.
- `consulta-mrr.sql`: MRR agrupado por plano (só matrículas `Ativa`), com linha de total geral via `GROUPING SETS`.

## Como rodar

### Opção rápida: `setup.sh`

```bash
bash setup.sh
```

Sobe SQL Server + API via Docker Compose, espera o banco ficar pronto, aplica `schema.sql` e `seed.sql` automaticamente. Idempotente — pode rodar de novo sem duplicar dados (pula o schema se o banco já existir). Ao final, mostra os próximos passos pro frontend.

Pré-requisitos: Docker Desktop com virtualização habilitada.

### Passo a passo manual

**1. Banco + API (Docker):**
```bash
docker compose up -d --build sqlserver api
```

**2. Aplicar schema e seed:**
```bash
docker exec -i pkm-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C < db/schema.sql
docker exec -i pkm-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C < db/seed.sql
```

API disponível em `http://localhost:5000` (Swagger em `/swagger`).

**3. Frontend:**
```bash
cd web
npm install
npm start
```
Acesse `http://localhost:4200`. `environment.ts` já aponta para `http://localhost:5000/api`.

**4. Rodar a API fora do Docker** (alternativa ao passo 1, se preferir `dotnet run` local com um SQL Server já disponível em `localhost:1433`):
```bash
cd api
dotnet restore && dotnet build
dotnet run --project src/PokemonTrainingCenter.Api
```

**5. Testes de unidade:**
```bash
cd api
dotnet test
```

## Endpoints da API

Documentação interativa completa em `http://localhost:5000/swagger`. Resumo:

**Matrículas**
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/matriculas` | Lista matrículas (filtros `busca` e `status`) |
| POST | `/api/matriculas` | Cria matrícula (valida R1 e R3) |
| POST | `/api/matriculas/{id}/upgrade/simular` | Calcula o pro-rata do upgrade **sem** efetivar (R2) |
| POST | `/api/matriculas/{id}/upgrade` | Efetiva o upgrade calculado (R2) |
| POST | `/api/matriculas/{id}/cancelar` | Cancela a matrícula (R4) |

**Pokémon**
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/pokemons` | Lista todos |
| GET | `/api/pokemons/{id}` | Obtém por id |
| POST | `/api/pokemons` | Cria um Pokémon |
| PUT | `/api/pokemons/{id}/transferir` | Transfere para outro Treinador (R5) |

**Treinadores**
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/treinadores` | Lista todos |
| GET | `/api/treinadores/{id}` | Obtém por id |
| POST | `/api/treinadores` | Cria um Treinador (valida e-mail único) |

**Planos de Treinamento**
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/planos-treinamento` | Lista os 3 planos |

## Decisões técnicas e premissas assumidas

- **R2 (upgrade pro-rata):** ciclo fixo de **30 dias** contado a partir da `DataInicio` da matrícula atual — é a suposição mais simples compatível com o exemplo do enunciado (dia 16/30 → R$35,00, validado nos testes). Upgrades pedidos após o fim do ciclo (>30 dias decorridos) não geram crédito nem cobrança negativa (fica R$0, nunca negativo). A UI segue o fluxo pedido no enunciado — **simular sem efetivar** (`POST /matriculas/{id}/upgrade/simular`) e só persistir de fato ao clicar em "Confirmar upgrade" (`POST /matriculas/{id}/upgrade`) — dois endpoints separados no backend para isso.
- **R3 (nível mínimo):** vale tanto na criação quanto no upgrade da matrícula (upgradar pra Elite dos 4 com Pokémon de nível insuficiente também é rejeitado).
- **R4 (matrículas canceladas):** o índice único de R1 é filtrado por `Status = 'Ativa'`, então uma matrícula `Cancelada` nunca bloqueia uma nova para o mesmo Pokémon. A consulta de MRR filtra explicitamente `Status = 'Ativa'`. Decisão: `Cancelada`/`Concluida` mantêm histórico (nunca são deletadas) e ficam de fora de qualquer somatório financeiro.
- **R5 (transferência de Pokémon):** a `Matricula` referencia o `Pokemon`, não o `Treinador`, diretamente. Por isso, transferir um Pokémon (`PUT /pokemons/{id}/transferir`) só precisa atualizar `Pokemon.TreinadorId` — nenhuma matrícula (ativa ou histórica) precisa ser tocada, pois o vínculo com o Treinador é sempre resolvido via `Pokemon.TreinadorId`. É o comportamento mais simples e coerente com o modelo: a matrícula "segue" o Pokémon.
- **Tipo do Pokémon:** modelado como enum fixo (`TipoPokemon`: Fogo, Água, Planta...) em vez de texto livre, para consistência e validação; mapeado como `NVARCHAR` no banco (não `INT`) via `HasConversion<string>()` no EF Core, para o schema continuar legível em SQL puro.

## O que eu faria diferente / melhoraria com mais tempo

- Telas dedicadas no Angular para criar Treinador e Pokémon (hoje só existem os endpoints na API — `POST /treinadores` e `POST /pokemons` — sem UI, já que o enunciado só pedia as telas de matrícula).
- Uma tela/confirmação para a transferência de Pokémon (R5) — hoje só testável via API/Swagger.
- Testes de integração da API (`WebApplicationFactory` contra um SQL Server real ou in-memory) e testes de componente no Angular (Karma/Jasmine ou migração pra Vitest). Hoje só existem os 15 testes de unidade do `MatriculaService`.
- Um pipeline de CI (GitHub/GitLab Actions) rodando `dotnet test`, `ng build` e o **build de produção do Docker** a cada push. Esse último item não é hipotético: o build de produção do `web/` (nginx) só foi validado manualmente numa revisão posterior, e só aí apareceram dois bugs reais (API inacessível por falta de proxy no nginx, e rotas do Angular quebrando em acesso direto) — um CI teria pego isso automaticamente, sem depender de alguém lembrar de testar aquele caminho.
- Healthcheck no `sqlserver` do `docker-compose.yml` (com `depends_on: condition: service_healthy`), em vez do polling manual que o `setup.sh` faz hoje — mais robusto que só esperar o container "iniciar".
- Paginação na listagem de matrículas.
- Modelar ciclos de cobrança recorrentes de verdade (hoje o pro-rata assume um único ciclo fixo de 30 dias a partir do início da matrícula; não há lógica de renovação mensal automática).
- EF Core Migrations em vez de `schema.sql` aplicado manualmente (mantive SQL puro porque é item explícito do desafio).
- Autenticação/autorização (a API está aberta; fora de escopo pro desafio, mas seria o próximo passo em um sistema real).

## Uso de IA

Usei o Claude Code do início ao fim deste projeto, em várias sessões de trabalho colaborativo (não foi "gerar tudo de uma vez"):

1. **Scaffold inicial:** pedi a estrutura em camadas dos três módulos (API .NET, Angular, SQL) a partir do enunciado. Nessa etapa, o `MatriculaService` ficou como stub (`NotImplementedException`) de propósito — a IA documentou isso com transparência no próprio código.
2. **Implementação das regras de negócio (R1-R4):** pedi para implementar o `MatriculaService` de verdade. Revisei a lógica do pro-rata manualmente conferindo contra o exemplo do enunciado.
3. **Validação de ponta a ponta contra banco real:** ao testar tudo via `curl` contra SQL Server real em Docker (não só compilar), a IA encontrou e corrigiu **3 bugs reais** que só apareciam em runtime:
   - Falta de `SET QUOTED_IDENTIFIER ON` antes de criar o índice filtrado no `schema.sql` (erro ao rodar o script do zero).
   - `Pokemon.Tipo` e `Matricula.Status` mapeados como `INT` pelo EF Core por padrão, mas armazenados como `NVARCHAR` no schema — quebrava toda leitura com `InvalidCastException`.
   - `GET /api/pokemons` retornando 500 por referência circular (`Pokemon → Treinador → Pokemons → ...`) ao serializar a entidade do EF Core direto em vez de um DTO.
4. **Frontend — tema Pokémon e modo claro/escuro:** pedi cores/tema temático, transições e um botão sol/lua, com a restrição explícita de usar só recursos nativos do Angular (signals, standalone components, `@angular/animations`), sem bibliotecas de UI novas. A IA implementou um `ThemeService` baseado em `signal`, CSS custom properties para os dois temas, e padronizou botões/cards/badges em classes globais reutilizáveis.
5. **Dados de teste:** pedi um `seed.sql` com 10 registros distintos e um `setup.sh` para automatizar o clone inicial. A IA testou os dois de ponta a ponta (inclusive dropando o banco pra simular um clone novo) antes de considerar pronto.
6. **Fechamento de lacunas (R5, testes, upgrade simular/confirmar):** pedi pra IA comparar o projeto inteiro contra o enunciado original de novo. Ela identificou, com honestidade, que R5 não tinha nenhum código (só uma decisão documentada), que o botão de Upgrade da listagem não estava ligado ao modal, e que o `POST /upgrade` efetivava a mudança na mesma chamada que calculava o valor — o que quebrava a UX pedida ("mostrar valor antes de confirmar"). A partir disso, separei o endpoint em `/upgrade/simular` (sem side-effect) e `/upgrade` (efetiva), implementei R5, liguei os botões que faltavam na listagem e escrevi os 15 testes de unidade que cobrem os casos de borda do pro-rata (primeiro dia, último dia, downgrade, nível insuficiente).

Todo o código foi revisado por mim antes de aceitar — nada foi commitado automaticamente pela IA sem eu pedir explicitamente.
