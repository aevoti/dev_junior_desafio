# Centro de Treinamento Pokémon Alto Nível

Aplicação full stack para gestão de treinadores, Pokémon e matrículas em planos mensais de treinamento, com regras de upgrade, cálculo proporcional, transferência entre treinadores e apuração de Receita Mensal Recorrente (MRR).

## Visão geral

A solução reúne uma API REST em .NET 8, persistência em SQL Server, uma interface em Angular 21, scripts SQL e testes automatizados. O sistema administra matrículas de Pokémon em planos de treinamento, preservando o histórico e aplicando regras de elegibilidade, cobrança, cancelamento, upgrade e transferência.

## Funcionalidades

### API

- Cadastro e listagem de treinadores.
- Cadastro e listagem de Pokémon.
- Transferência de Pokémon entre treinadores.
- Listagem dos planos de treinamento.
- Criação e cancelamento de matrículas.
- Busca de matrículas por Pokémon ou treinador e filtro por status.
- Simulação e efetivação de upgrade de plano.
- Respostas de erro padronizadas e documentação interativa por Swagger.

### Frontend

- Listagem de matrículas e cards de resumo.
- Busca por Pokémon ou treinador e filtro por status.
- Criação de matrícula com validação de nível.
- Simulação e confirmação de upgrade.
- Transferência visual de um Pokémon para outro treinador.
- Feedback amigável de sucesso e erro.
- Layout responsivo e enriquecimento visual opcional pela PokéAPI.

O frontend não possui telas de cadastro de treinadores ou Pokémon; esses recursos podem ser cadastrados pela API. Essa foi uma decisão de escopo, pois as telas mínimas solicitadas estavam concentradas na gestão das matrículas.

## Tecnologias

### Backend

- .NET 8, C# e ASP.NET Core Web API.
- Entity Framework Core 8 e SQL Server.
- Swagger/OpenAPI.
- xUnit e EF Core InMemory nos testes de services.

### Frontend

- Angular 21 com componentes standalone.
- TypeScript, Reactive Forms e HttpClient.
- SCSS.

### Integração e ferramentas

- PokéAPI, apenas para enriquecimento visual.
- Git, npm e `sqlcmd`.

## Estrutura do projeto

```text
backend/
├── PokemonTraining.Api/       # API, entidades, DTOs, services e acesso a dados
└── PokemonTraining.Tests/     # testes automatizados das regras de negócio
frontend/                      # aplicação Angular
database/
├── schema.sql                 # banco, tabelas, constraints, índices e planos
├── consulta-mrr.sql           # consulta da Receita Mensal Recorrente
└── demo-data.sql              # dados opcionais para demonstração
docs/
└── DESAFIO_ORIGINAL.md        # enunciado original preservado
PokemonTraining.sln            # solução .NET
README.md                       # documentação da solução
```

Diretórios gerados, como `bin`, `obj`, `node_modules`, `dist` e `.angular`, não fazem parte da estrutura versionada.

## Pré-requisitos

- .NET SDK 8.
- Node.js em versão compatível com Angular 21.
- npm.
- SQL Server.
- Git.
- `sqlcmd` opcional, recomendado para executar os exemplos deste documento pelo terminal.

O projeto foi validado localmente com .NET SDK 8.0.423, Node.js 24.14.0, npm 11.9.0 e SQL Server 2025 Developer Edition. Essas versões exatas não são obrigatórias, desde que as ferramentas sejam compatíveis.

## Banco de dados

A conexão local está configurada em `backend/PokemonTraining.Api/appsettings.json`:

```text
Server=localhost;Database=PokemonTrainingDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Ela usa autenticação integrada do sistema operacional. Não há credenciais ou senhas reais no repositório.

### Criação do schema

Na raiz do repositório:

```powershell
sqlcmd -S localhost -E -b -i database\schema.sql
```

O script `database/schema.sql` cria o banco caso necessário; cria tabelas, chaves, constraints e índices ausentes; cria o índice único filtrado da R1; e insere os três planos quando seus IDs ainda não existem. Ele foi estruturado e validado para poder ser repetido no schema esperado, mas não substitui uma ferramenta de migração para reconciliar estruturas previamente modificadas.

### Dados de demonstração opcionais

`database/demo-data.sql` contém dados demonstrativos para facilitar a avaliação visual. É opcional, não faz parte da estrutura obrigatória, não substitui `schema.sql` e deve ser executado depois dele:

```powershell
sqlcmd -S localhost -E -b -d PokemonTrainingDb -i database\demo-data.sql
```

O script verifica os registros que insere, não apaga dados existentes e pode ser repetido no conjunto demonstrativo esperado. Seus personagens e nomes são exemplos, não regras do sistema. Em terminais cuja página de código não preserve UTF-8, pode-se acrescentar `-f 65001` para proteger caracteres acentuados.

### Consulta de MRR

```powershell
sqlcmd -S localhost -E -b -d PokemonTrainingDb -i database\consulta-mrr.sql
```

`database/consulta-mrr.sql` considera somente matrículas `Ativa`, agrupa por plano e acrescenta a linha `TOTAL GERAL`. Matrículas `Cancelada` e `Concluida` não entram. A soma utiliza `Matricula.ValorMensal`, preservando o preço contratado independentemente de alterações futuras no plano.

## Execução do backend

Na raiz:

```powershell
dotnet restore PokemonTraining.sln
dotnet build PokemonTraining.sln
dotnet run --project backend/PokemonTraining.Api/PokemonTraining.Api.csproj --launch-profile http
```

Com o perfil atual, a API fica em `http://localhost:5234` e o Swagger em `http://localhost:5234/swagger/index.html`. A porta pode variar se `launchSettings.json` for alterado.

## Execução do frontend

Com a API em `http://localhost:5234`:

```powershell
cd frontend
npm install
npm start
```

A interface fica em `http://localhost:4200`. A URL da API está centralizada em `frontend/src/environments/environment.ts`.

## Testes automatizados

```powershell
dotnet test PokemonTraining.sln
```

O estado atual possui 23 testes automatizados. Eles cobrem, entre outros: matrícula ativa duplicada; nova matrícula após cancelamento; nível mínimo; cancelamento; pró-rata; downgrade e mesmo plano; simulação e efetivação de upgrade; transferência; preservação das matrículas na R5; e recursos inexistentes na transferência.

O EF Core InMemory é usado nos testes unitários de services, mas não reproduz integralmente índices filtrados, concorrência real ou transações relacionais do SQL Server. Esses comportamentos foram complementados por validações reais no SQL Server.

## Endpoints

### Treinadores

| Método | Rota | Finalidade |
|---|---|---|
| `GET` | `/api/treinadores` | Listar treinadores |
| `POST` | `/api/treinadores` | Cadastrar treinador |

### Pokémon

| Método | Rota | Finalidade |
|---|---|---|
| `GET` | `/api/pokemons` | Listar Pokémon |
| `POST` | `/api/pokemons` | Cadastrar Pokémon |
| `PATCH` | `/api/pokemons/{id}/transferencia` | Transferir Pokémon |

### Planos

| Método | Rota | Finalidade |
|---|---|---|
| `GET` | `/api/planos` | Listar planos |

### Matrículas

| Método | Rota | Finalidade |
|---|---|---|
| `GET` | `/api/matriculas` | Listar, buscar e filtrar |
| `POST` | `/api/matriculas` | Criar matrícula |
| `PATCH` | `/api/matriculas/{id}/cancelamento` | Cancelar matrícula |
| `POST` | `/api/matriculas/{id}/simular-upgrade` | Simular upgrade |
| `POST` | `/api/matriculas/{id}/upgrade` | Efetivar upgrade |

Exemplo: `GET /api/matriculas?busca=pikachu&status=Ativa`.

## Planos de treinamento

| Plano | Valor mensal | Ordem | Nível mínimo | Descrição |
|---|---:|---:|---:|---|
| Ginásio Local | R$ 50,00 | 1 | 1 | Treinos básicos |
| Liga Regional | R$ 120,00 | 2 | 1 | Treinos intermediários e batalhas simuladas |
| Elite dos 4 | R$ 300,00 | 3 | 50 | Preparação completa para a Liga |

## Regras de negócio

### R1 — Matrícula ativa única

O service verifica se o Pokémon já possui matrícula ativa. No SQL Server, um índice único filtrado em `Matriculas(PokemonId)` para `Status = 'Ativa'` complementa a validação e protege a integridade sob concorrência. Uma tentativa duplicada retorna conflito amigável (`409`).

### R2 — Upgrade e pró-rata

O upgrade aceita apenas plano de `Ordem` superior: o mesmo plano e downgrade são recusados, o nível mínimo é verificado e somente matrícula ativa pode receber upgrade. Na efetivação, a anterior fica `Concluida` e uma nova matrícula `Ativa` é criada. No SQL Server, a operação é transacional.

O ciclo é fixo em 30 dias:

```text
creditoAnterior  = valorAnterior × diasRestantes / 30
custoNovo        = valorNovo × diasRestantes / 30
primeiraCobranca = custoNovo - creditoAnterior
```

Os valores são arredondados em duas casas e a cobrança nunca fica negativa. Do Ginásio Local (R$ 50,00) para a Liga Regional (R$ 120,00), com 15 dias restantes, o crédito é R$ 25,00, o novo custo proporcional é R$ 60,00 e a primeira cobrança é R$ 35,00.

A primeira cobrança é retornada, não persistida; `ValorMensal` da nova matrícula guarda o preço mensal cheio.

#### Premissas do ciclo

- Ciclos sucessivos de 30 dias.
- O dia do upgrade inicia o novo plano.
- Upgrade na data inicial possui 30 dias restantes.
- Exatamente 30 dias após o início representa um novo ciclo.
- A nova matrícula começa na data do upgrade.

### R3 — Nível mínimo

A regra usa `Pokemon.Nivel >= PlanoTreinamento.NivelMinimo`. Ela não depende do nome “Elite dos 4”, permitindo novos planos com outros requisitos.

### R4 — Matrículas canceladas

Permanecem no histórico, recebem `Status = Cancelada` e `DataFim`, e aceitam `MotivoEncerramento` opcional. Não são removidas, não bloqueiam nova matrícula e não entram no MRR.

### R5 — Transferência

A matrícula pertence ao Pokémon. A transferência altera apenas `Pokemon.TreinadorId`; IDs e quantidade de matrículas, planos, status, valores, datas e motivos permanecem íntegros. Está disponível na API e no Angular:

```http
PATCH /api/pokemons/{id}/transferencia
Content-Type: application/json

{
  "novoTreinadorId": 2
}
```

O frontend exclui o treinador atual das opções. No sucesso, fecha o modal e atualiza a lista preservando os filtros.

## PokéAPI

A PokéAPI fornece apenas sprites e tipos para enriquecimento visual. A integração normaliza o nome, mantém cache em memória, trata indisponibilidade silenciosamente e usa fallback em CSS. Não é a fonte oficial dos cadastros nem dependência para matrículas, upgrades, transferências, consultas internas ou regras. Se estiver indisponível, a aplicação continua funcional.

## Tratamento de erros

```json
{
  "message": "Mensagem clara para o usuário."
}
```

| Status | Situação |
|---:|---|
| `400` | Validação ou regra de negócio |
| `404` | Recurso não encontrado |
| `409` | Conflito |
| `500` | Erro inesperado |

Stack traces não são expostos; o frontend apresenta mensagens amigáveis.

## Decisões técnicas

- Arquitetura simples: controllers enxutos, regras em services e contratos em DTOs.
- Entity Framework Core com `StatusMatricula` persistido como string.
- Índice filtrado para complementar a R1.
- `ValorMensal` preservado na matrícula.
- Transação no upgrade e UTC para datas geradas pela aplicação.
- Angular standalone e Reactive Forms.
- CORS restrito a `http://localhost:4200`.
- PokéAPI opcional.
- Sem autenticação, pois não fazia parte do escopo.

## Limitações

- Sem autenticação/autorização ou paginação.
- Cadastro de treinador e Pokémon somente pela API.
- Cancelamento sem fluxo visual dedicado.
- Sem migrations entregues.
- Frontend sem suíte extensa de testes automatizados.
- Testes unitários com EF Core InMemory.
- Concorrência simultânea sem teste automatizado.
- Rollback forçado não validado manualmente.
- Primeira cobrança não persistida.

O foco foi o escopo obrigatório e as regras centrais.

## Melhorias futuras

- Autenticação, autorização e paginação.
- Migrations e configuração por ambientes.
- Testes de integração com SQL Server e end-to-end do frontend.
- Telas administrativas e fluxo visual de cancelamento.
- Histórico de cobranças.
- Containerização.

## Uso de inteligência artificial

Ferramentas de IA generativa foram usadas como assistência para analisar o enunciado, organizar etapas, discutir modelagem, revisar regras, identificar casos de borda, sugerir testes, investigar erros e revisar documentação.

As sugestões precisaram de validação e adaptação. Exemplos reais:

- correção da codificação de “Ginásio Local” ao executar SQL por `sqlcmd`;
- uso de `sqlcmd -b` para falhar corretamente em erros SQL;
- escolha de Angular compatível com Node.js 24;
- ajuste da atualização de estado no Angular zoneless;
- correção da persistência dos filtros após criação e upgrade;
- refinamento das regras e testes da R5;
- revisão do pró-rata;
- integração da PokéAPI com fallback e cache.

Todas as sugestões utilizadas foram revisadas, compreendidas, testadas e ajustadas durante o desenvolvimento. A assistência de IA não substituiu as decisões técnicas nem a validação da solução.

## Desafio original

O enunciado original está preservado em [docs/DESAFIO_ORIGINAL.md](docs/DESAFIO_ORIGINAL.md).
