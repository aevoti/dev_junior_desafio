# Centro de Treinamento Pokémon "Alto Nível"

Sistema de gestão de matrículas de Pokémon em planos de treinamento mensais, desenvolvido como desafio técnico.

## 🧱 Stack

- **Backend:** .NET 10 / ASP.NET Core Web API
- **Banco de dados:** SQL Server
- **ORM:** Entity Framework Core
- **Frontend:** Angular
- **Testes:** xUnit + Moq

---

## ▶️ Como executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local, Docker ou SQL Server Express)
- [Node.js](https://nodejs.org/) 18+ e Angular CLI (`npm install -g @angular/cli`)

### Backend

1. Configure a connection string em `backend/src/PokemonCenter.Api/appsettings.json`:
```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=PokemonTrainingCenter;Trusted_Connection=True;TrustServerCertificate=True;"
   }
```

2. Restaure e crie o banco via migrations do EF Core:
```bash
   cd backend
   dotnet tool install --global dotnet-ef   # se ainda não tiver instalado
   dotnet ef migrations add InitialCreate -p src/PokemonCenter.Infrastructure -s src/PokemonCenter.Api
   dotnet ef database update -p src/PokemonCenter.Infrastructure -s src/PokemonCenter.Api
```

   **Alternativa manual:** em vez das migrations, você pode rodar os scripts em `/database` diretamente no SQL Server (`schema.sql`, depois `seed.sql` opcionalmente).

3. Rode a API:
```bash
   cd src/PokemonCenter.Api
   dotnet run
```
   A documentação interativa fica disponível em `https://localhost:<porta>/scalar/v1` (a porta exata aparece no terminal ao iniciar).

### Frontend

```bash
cd frontend/pokemon-training-center-ui
npm install
ng serve
```
Acesse `http://localhost:4200`.

### Banco de dados (scripts SQL puro)

Localizados em `/database`:
- `schema.sql` — criação das tabelas, constraints e o índice único filtrado que reforça R1 no próprio banco.
- `seed.sql` — dados de teste cobrindo os casos de borda das regras de negócio.
- `consulta-mrr.sql` — consulta de Receita Mensal Recorrente por plano, com total geral.

---

## 🧩 Decisões técnicas e premissas assumidas

### R2 — Cálculo pro-rata do upgrade
O ciclo de cobrança é assumido como **fixo em 30 dias, contado a partir da `DataInicio` da matrícula atual** (conforme o exemplo do enunciado). **Limitação conhecida:** essa implementação não trata múltiplos ciclos/renovações — se o Pokémon estiver matriculado há mais de 30 dias, o cálculo considera o ciclo já esgotado (crédito e cobrança zerados) em vez de recalcular a partir do início do ciclo vigente. Em produção, seria necessário um campo de "início do ciclo atual" separado da data original da matrícula.

O valor mensal de cada matrícula é **congelado no momento da criação** (`Matricula.ValorMensal`), em vez de sempre consultar o valor atual do plano — isso evita que mudanças futuras de preço afetem retroativamente matrículas já ativas.

### R3 — Tipo do Pokémon como enum
`Tipo` foi modelado como um enum fechado (`TipoPokemon`) em vez de string livre, para evitar inconsistências (`"Fogo"` vs `"fogo"`) e refletir que o enunciado sugere um conjunto fechado de tipos. **Premissa assumida:** cada Pokémon possui **apenas um tipo** (não foi modelado tipo duplo, como Fogo/Voador, por não estar no escopo do enunciado).

### R4 — Matrículas canceladas
Matrículas com `Status = Cancelada` não são excluídas do banco (mantidas para histórico), mas são **sempre filtradas fora** de qualquer relatório de receita ativa — inclusive na consulta de MRR (`consulta-mrr.sql`), que considera apenas `Status = 'Ativa'`.

### R5 — Transferência de Pokémon entre Treinadores
Optamos pela abordagem mais simples: ao transferir um Pokémon para outro Treinador (`PATCH /api/pokemons/{id}/transferir`), a matrícula ativa (se existir) **permanece vinculada ao Pokémon** e passa a ser "herdada" pelo novo Treinador — não há cancelamento automático. Essa foi uma decisão de escopo; uma abordagem mais conservadora (cancelar a matrícula na transferência) também seria válida.

### Índice único filtrado (R1 no banco)
Além da validação na camada de Application, o `schema.sql` inclui um índice único filtrado (`WHERE Status = 'Ativa'`) na tabela `Matriculas`, reforçando a regra de matrícula única ativa diretamente no banco — protege contra inconsistências mesmo em cenários de acesso concorrente ou escrita direta no banco.

---

## 🔍 O que eu faria diferente ou melhoraria com mais tempo

- Implementar corretamente a renovação de múltiplos ciclos no cálculo pro-rata (hoje simplificado, ver limitação acima).
- Testes de integração para os Controllers e Repositórios (hoje a cobertura é só de testes unitários do `MatriculaService`, via mocks).
- Autenticação/autorização (fora do escopo do desafio, mas necessário em produção).
- Paginação na listagem de matrículas.
- Modelar tipo duplo de Pokémon (Fogo/Voador, etc.), caso fosse exigido.
- Migrar a versão do `Microsoft.OpenApi` puxada transitivamente pelo `Scalar.AspNetCore`, que ainda gera um warning de segurança (`NU1903`) por depender de uma versão desatualizada do pacote.

---

## 🤖 Uso de Inteligência Artificial

Utilizei o Claude (Anthropic) ao longo de todo o desenvolvimento, com os seguintes usos principais:
- Revisão das decisões da arquitetura de pastas do backend (camadas Domain/Application/Infrastructure/Api) e do frontend Angular.
- Geração dos testes unitários (xUnit + Moq) cobrindo R1, R2, R3 e casos de borda.
- Depuração de problemas de ambiente (SDK não encontrado, conflito de versão do `Microsoft.OpenApi` com o gerador de OpenAPI nativo do .NET 10).

Todo o código gerado foi revisado, compilado e testado localmente antes de ser incorporado — inclusive corrigindo bugs identificados durante a revisão (ex.: o cálculo de dias restantes não tratava upgrades solicitados após o fim do ciclo).