# CLAUDE.md — Frontend (Angular)

Este arquivo orienta o Claude Code no escopo do frontend deste projeto. O backend já está pronto e roda em `http://localhost:5289` (ou `https://localhost:7286`).

## Contexto do projeto

Frontend Angular para o "Centro de Treinamento Pokémon Alto Nível" — sistema de gestão de matrículas de Pokémon em planos de treinamento mensais. Desafio técnico para vaga de desenvolvedor(a) júnior. O enunciado original e completo está em `Docs/desafio-original.md` (na raiz do repo). O roadmap de construção está em `FrontEnd/guia.md` — siga essa ordem ao propor ou implementar código, a menos que o usuário peça o contrário.

## Stack

- Angular, versão estável mais recente, **standalone components** (sem `NgModule`).
- **Reactive Forms** (`ReactiveFormsModule`) para os formulários, aproveitando `Validators` para as validações obrigatórias do desafio.
- `provideHttpClient()` para chamadas à API — um service por recurso (Treinador, Pokémon, Plano, Matrícula), sem lógica de negócio duplicada no componente.
- Injeção via função `inject()`, não construtor, seguindo o padrão atual do Angular.
- Signals para estado local dos componentes onde fizer sentido (listas, loading, erro).

## Contrato da API (backend já implementado)

Base URL: `http://localhost:5289`. CORS já liberado para `http://localhost:4200` (porta padrão do `ng serve`) — se o front rodar em outra porta, é preciso ajustar `Program.cs` no backend.

### `GET /api/planos`
Retorna `{ id, nome, descricao, valorMensal }[]`. Os 3 planos já vêm seedados no banco (Ginásio Local R$50, Liga Regional R$120, Elite dos 4 R$300). Use esse endpoint para popular os selects — **não hardcode IDs de plano no frontend**.

### `GET/POST /api/treinadores`, `GET /api/treinadores/{id}`
`POST` body: `{ nome, email, cidadeDeOrigem }` → `{ id, nome, email, cidadeDeOrigem }`. Sem PUT/DELETE (fora de escopo do desafio).

### `GET/POST /api/pokemons`, `GET /api/pokemons/{id}`, `PATCH /api/pokemons/{id}/transferir`
`POST` body: `{ nome, tipo, nivel, treinadorId }` → `{ id, nome, tipo, nivel, treinadorId }`. `PATCH .../transferir` body: `{ novoTreinadorId }`.

### `GET/POST /api/matriculas`, `GET /api/matriculas/{id}`
`POST` body: `{ pokemonId, planoId }`.
Resposta (`MatriculaResponse`) já vem **desnormalizada** — inclui `pokemonNome`, `treinadorId`, `treinadorNome`, `planoNome`, `planoValorMensal` junto com os IDs. A tela de listagem não precisa cruzar dados com outros endpoints.
`status` é sempre uma destas strings exatas: `"Ativa"`, `"Cancelada"`, `"Concluida"` (sem acento em Concluida).

### `PATCH /api/matriculas/{id}/cancelar`
Sem body. Só funciona se a matrícula estiver `Ativa`.

### `GET /api/matriculas/{id}/upgrade?novoPlanoId={id}`
**Simula** o upgrade sem persistir nada. Retorna `{ primeiraCobranca }` (decimal). Use isso para o passo de confirmação do fluxo de upgrade (R2) antes do usuário confirmar.

### `PATCH /api/matriculas/{id}/upgrade`
Body: `{ novoPlanoId }`. **Confirma** o upgrade: encerra a matrícula atual (`Concluida`) e cria uma nova `Ativa` no plano novo. Chame isso só depois do usuário confirmar o valor simulado no endpoint acima.

## Regras de negócio que o frontend precisa refletir (ver Docs/desafio-original.md para o texto completo)

- **R1**: um Pokémon não pode ter duas matrículas ativas. A API rejeita com 400 — exiba a mensagem de erro do backend de forma amigável (não precisa reimplementar a checagem no client, mas pode desabilitar a opção na UI se for óbvio, ex.: não listar pokémon já com matrícula ativa no formulário de nova matrícula).
- **R2**: upgrade só para plano de valor mensal maior. Downgrade é rejeitado pela API — o formulário de upgrade deve listar como opção só os planos com `valorMensal` maior que o da matrícula atual.
- **R3**: nível mínimo 50 para o plano "Elite dos 4". Valide isso no formulário (client-side, pra UX melhor) **e** trate o erro 400 que a API retorna se a validação client-side for burlada.
- **R4**: matrículas `Cancelada`/`Concluida` não devem ser tratadas como ativas em nenhuma tela (ex.: contagem, badges, filtros default).
- **R5**: transferência de Pokémon (`PATCH /api/pokemons/{id}/transferir`) deve refletir os filtros de busca.

## Regras do que NÃO fazer

- Não implementar autenticação, login ou rotas protegidas — fora de escopo.
- Não usar `NgModule` — o padrão deste projeto é standalone components.
- Não hardcodar IDs ou valores de planos — sempre buscar via `GET /api/planos`.
- Não reimplementar validação de negócio (R1-R3) só no frontend sem tratar o erro que a API retorna — a API é a fonte da verdade; validação client-side é só UX.
- Não investir tempo em beleza visual — o enunciado deixa claro que isso não é avaliado. Priorize organização de componentes, validações e tratamento de erro.
- Não inventar telas além das 4 mínimas pedidas sem necessidade (listagem, nova matrícula, upgrade, tratamento de erro) — telas de apoio (cadastro de Treinador/Pokémon) só existem porque são pré-requisito funcional pra popular os selects, não porque foram pedidas como entregável separado.

## Convenções

- Nomes de variáveis, componentes, rotas e mensagens em português, consistente com o domínio do desafio e com o backend (Treinador, Pokemon, Matricula, Plano).
- Nomes de arquivo e seletor de componente seguem o padrão Angular (`kebab-case`, ex.: `matricula-list.component.ts`, seletor `app-matricula-list`).
- Models TypeScript (interfaces) devem espelhar exatamente os DTOs de resposta da API listados acima.
