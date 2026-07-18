# Quickstart: Gestão de Matrículas do Centro de Treinamento Pokémon

Guia de validação ponta a ponta desta feature. Não repete comandos exatos de
setup de projeto (isso é gerado pelas tarefas de implementação em
`tasks.md`) — assume que `backend/`, `frontend/` e `database/` já existem.

## Pré-requisitos

- .NET 8 SDK
- Docker Desktop (para o container do SQL Server — funciona em Windows,
  Mac ou Linux)
- Node.js LTS + Angular CLI (`npm install -g @angular/cli`)

## Subindo o banco de dados

```powershell
docker compose up -d db
```

Isso sobe o único container do projeto (SQL Server). Backend e frontend
rodam nativos, fora de container (ver `research.md` item 8).

## Subindo o backend

```powershell
cd backend/src/PokemonTrainingCenter.Api
dotnet ef database update   # aplica as migrations (cria o schema) contra o container
dotnet run
```

A API deve responder em `https://localhost:5001` (ou porta configurada em
`launchSettings.json`). `GET /api/training-plans` deve retornar os 3 planos
seed (ver `data-model.md`).

## Rodando os testes de backend (obrigatório — Princípio I)

```powershell
cd backend
dotnet test
```

Todos os testes de `PokemonTrainingCenter.UnitTests` cobrindo R1, R2, R3 e
seus casos de borda (ver `spec.md` Edge Cases) devem passar, incluindo o
caso numérico do próprio enunciado (upgrade no dia 16 de um ciclo de 30
dias → R$ 35,00).

## Subindo o frontend

```powershell
cd frontend
npm install
ng serve
```

Acesse `http://localhost:4200`.

## Cenários de validação manual (golden path)

Cada cenário referencia a User Story e o Acceptance Scenario correspondente
em `spec.md`.

1. **Matricular um Pokémon (US1)**: cadastre um Treinador, cadastre um
   Pokémon de nível 30 vinculado a ele, matricule-o no plano "Ginásio
   Local". A matrícula deve aparecer com status "Ativa" e valor R$ 50,00,
   sem exigir preenchimento de data de início.
2. **Rejeição de matrícula duplicada (US1, cenário 2 — R1)**: tente
   matricular o mesmo Pokémon novamente em qualquer plano. A API deve
   retornar `409` e o frontend deve exibir a mensagem de erro de forma
   amigável, não como um erro técnico genérico.
3. **Nível mínimo para Elite dos 4 (US1, cenários 3-4 — R3)**: tente
   matricular um Pokémon de nível 40 no plano "Elite dos 4" (deve ser
   rejeitado); repita com nível 50 (deve ser aceito).
4. **Upgrade com pro-rata (US2, cenário 1 — R2)**: com a matrícula do passo
   1 ainda ativa, solicite upgrade para "Liga Regional" simulando 15 dias
   restantes de ciclo. O preview deve mostrar R$ 35,00 antes de qualquer
   confirmação; ao confirmar, a matrícula antiga deve aparecer como
   encerrada e uma nova como ativa no novo plano.
5. **Downgrade rejeitado (US2, cenário 3)**: tente "upgrade" de um plano
   superior para um inferior — deve ser rejeitado.
6. **Busca e filtro (US3)**: na listagem, busque por parte do nome do
   Pokémon sem acentos/maiúsculas corretas (ex. "agua" para um Pokémon
   "Água-viva") e confirme que ele aparece; filtre por status "Ativa" e
   confirme que matrículas encerradas somem da lista.
7. **Cancelamento (US4, cenário 1 — R4)**: cancele uma matrícula ativa;
   confirme que ela passa a aparecer como "A encerrar" até a data de fim do
   ciclo, e não entra mais nos cálculos de receita ativa depois dela.
8. **Transferência com recriação automática (US4, cenário 3 — R5)**:
   transfira um Pokémon com matrícula ativa para outro Treinador; confirme
   que a matrícula antiga aparece encerrada com data de término igual à
   data da transferência, e que uma nova matrícula ativa no mesmo plano foi
   criada automaticamente sob o novo Treinador — sem nenhuma ação manual
   adicional.
9. **Consulta de MRR (US5)**: rode `database/consulta-mrr.sql` diretamente
   no SQL Server contra os dados criados nos passos acima. Confirme que a
   soma por plano considera apenas matrículas ativas e que existe uma linha
   de Total Geral, mesmo que algum plano não tenha nenhuma matrícula ativa.

Se todos os 9 cenários passarem, a feature está pronta para revisão de
código antes da entrega.
