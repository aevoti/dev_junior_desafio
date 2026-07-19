# Centro de Treinamento Pokémon "Alto Nível"

Sistema de gestão de matrículas do desafio técnico da Aevo (vaga Desenvolvedor(a) Junior). Este README documenta a **estrutura/arquitetura inicial** do projeto: como ele está organizado, as tecnologias usadas em cada módulo e o que já existe vs. o que ainda falta implementar.

## Organização do repositório

Monorepo com separação clara por módulo (backend, frontend, banco), cada um na sua própria stack:

```
dev_junior_desafio/
├── api/                 <- backend (.NET / ASP.NET Core Web API)
├── web/                 <- frontend (Angular)
├── db/                  <- scripts SQL puro (schema.sql, consulta-mrr.sql)
├── docker-compose.yml   <- sobe SQL Server + api + web juntos
└── README.md            <- este arquivo
```

### `api/` — Backend (.NET 8 / ASP.NET Core Web API)

Arquitetura em camadas (Clean Architecture simplificada):

```
api/
├── PokemonTrainingCenter.sln
├── Dockerfile
├── src/
│   ├── PokemonTrainingCenter.Api/              <- Controllers, Program.cs, appsettings, Swagger
│   ├── PokemonTrainingCenter.Domain/           <- Entidades (Treinador, Pokemon, PlanoTreinamento, Matricula), enums, exceções de domínio
│   ├── PokemonTrainingCenter.Application/      <- DTOs, interfaces de serviço/repositório, MatriculaService (regras de negócio)
│   └── PokemonTrainingCenter.Infrastructure/   <- AppDbContext (EF Core), implementação dos repositórios
└── tests/
    └── PokemonTrainingCenter.Tests/            <- projeto de testes (xUnit)
```

**Tecnologias:** .NET 8, ASP.NET Core Web API, Entity Framework Core (SQL Server), Swashbuckle (Swagger), xUnit.

### `web/` — Frontend (Angular)

Standalone components (Angular 17+, sem `NgModule`), organizado por *feature*:

```
web/
├── Dockerfile
├── package.json / angular.json / tsconfig*.json
└── src/app/
    ├── core/
    │   ├── models/         <- interfaces TypeScript espelhando os DTOs da API
    │   ├── services/       <- MatriculaService, PlanoTreinamentoService (HttpClient)
    │   └── interceptors/   <- errorInterceptor (normaliza erros da API em mensagens amigáveis)
    ├── features/matriculas/
    │   ├── pages/matriculas-list/     <- Tela 1: listagem + busca + filtro por status
    │   ├── pages/matricula-form/      <- Tela 2: formulário de nova matrícula + validações
    │   └── components/upgrade-modal/  <- Tela 3: fluxo de upgrade com valor pro-rata
    └── shared/components/
```

**Tecnologias:** Angular 17 (standalone components), TypeScript, RxJS, HttpClient com interceptor funcional.

### `db/` — SQL Server

- `schema.sql`: DDL das 4 tabelas (`Treinadores`, `Pokemons`, `PlanosTreinamento`, `Matriculas`), com seed dos 3 planos e um índice único filtrado que reforça a R1 (uma matrícula ativa por Pokémon) também a nível de banco.
- `consulta-mrr.sql`: consulta de Receita Mensal Recorrente agrupada por plano (apenas matrículas `Ativa`), com linha de total geral via `GROUPING SETS`.

## Status atual da implementação

Esta entrega cobre a **estrutura/arquitetura inicial** dos três módulos — não a implementação completa das regras de negócio. Especificamente:

| Item | Status |
|---|---|
| Modelagem de entidades (Domain) | ✅ Feito |
| Schema do banco (`schema.sql`) | ✅ Feito |
| Consulta de MRR (`consulta-mrr.sql`) | ✅ Feito |
| Controllers e rotas da API (CRUD básico de Treinador/Pokémon/Plano) | ✅ Feito |
| Lógica de `MatriculaService` (R1, R2, R3, R4) | ⏳ Stub com `NotImplementedException` e TODOs mapeando cada regra |
| Middleware de tratamento de erro (`DomainException` → HTTP 400 amigável) | ✅ Feito |
| Telas Angular (listagem, formulário, modal de upgrade) | ⏳ Estrutura e chamadas HTTP prontas; validações básicas de formulário feitas; falta ligar o fluxo de upgrade fim-a-fim |
| Interceptor de erro amigável no frontend | ✅ Feito |
| Endpoint de transferência de Pokémon (R5) | ⏳ Não implementado (ver premissa abaixo) |

## Decisões e premissas assumidas

- **R4 (matrículas canceladas):** o índice único de R1 é filtrado por `Status = 'Ativa'`, então uma matrícula `Cancelada` nunca bloqueia uma nova. A consulta de MRR filtra explicitamente `WHERE Status = 'Ativa'`. Decisão: `Cancelada` mantém o histórico (não é deletada) e fica de fora de qualquer somatório financeiro.
- **R5 (transferência de Pokémon):** a `Matricula` referencia o `Pokemon`, não o `Treinador`, diretamente — o vínculo com o treinador é sempre via `Pokemon.TreinadorId`. Decisão assumida: ao transferir um Pokémon, a matrícula ativa **continua ativa** e passa a "pertencer" ao novo treinador automaticamente (por ser uma FK indireta), sem precisar duplicar ou recriar a matrícula. Esse é o comportamento mais simples e coerente com o modelo, mas fica marcado como decisão a validar/ajustar quando a regra for implementada.
- **Fluxo de upgrade no frontend:** hoje o endpoint `POST /matriculas/{id}/upgrade` calcula **e já efetiva** o upgrade na mesma chamada. Para a UX pedida ("exibir o valor antes de confirmar") o ideal é separar em endpoint de simulação (sem side-effect) + endpoint de confirmação — está documentado como próximo passo, não implementado ainda.

## Como rodar (setup local)

> Pré-requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download), [Node.js 20+](https://nodejs.org/), Docker (opcional, para SQL Server em container).

### 1. Banco de dados

```bash
# Opção A: via Docker Compose (sobe SQL Server + api + web juntos)
docker-compose up -d sqlserver

# Opção B: apontar para uma instância local de SQL Server já existente
```

Depois de o banco estar no ar, execute o script de schema:

```bash
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -i db/schema.sql
```

### 2. Backend (api/)

```bash
cd api
dotnet restore
dotnet build
dotnet run --project src/PokemonTrainingCenter.Api
```

A API sobe em `http://localhost:5000` com Swagger em `/swagger`. Ajuste a connection string em `api/src/PokemonTrainingCenter.Api/appsettings.json` se necessário.

### 3. Frontend (web/)

```bash
cd web
npm install
npm start
```

Acessa em `http://localhost:4200`. O `environment.ts` já aponta para `http://localhost:5000/api`.

### 4. Tudo junto via Docker Compose

```bash
docker-compose up -d
```

## O que eu faria diferente / melhoraria com mais tempo

- Implementar de fato o `MatriculaService` (R1-R4) e escrever os testes de unidade cobrindo os casos de borda do pro-rata (upgrade no primeiro dia, no último dia, mesmo dia do ciclo).
- Separar o endpoint de upgrade em "simular" (GET, sem side-effect) e "confirmar" (POST), para a UI conseguir mostrar o valor sem já efetivar a operação.
- Adicionar EF Core Migrations em vez de `schema.sql` aplicado manualmente (mantive SQL puro aqui porque é um item explícito do desafio).
- Endpoint e tela dedicados para transferência de Pokémon (R5), com confirmação explícita do usuário.
- Testes de integração da API (WebApplicationFactory) e testes de componente no Angular (Jasmine/Karma ou migração para Vitest).
- Paginação na listagem de matrículas.

## Uso de IA

Este scaffold inicial (estrutura de pastas, csproj, entidades, DbContext, controllers stub, schema SQL, consulta de MRR e o esqueleto Angular) foi gerado com apoio do Claude Code, a partir do enunciado do desafio. O que foi pedido: montar a separação de módulos (API .NET / Frontend Angular / scripts SQL) já na arquitetura em camadas, e documentar tudo de forma transparente neste README, incluindo o que ainda é apenas *stub*/TODO. Nenhuma regra de negócio (R1-R5) foi implementada de fato nesta etapa — isso fica para a próxima iteração, que será feita manualmente revisando e completando cada TODO.
