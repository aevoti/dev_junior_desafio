# API Contract: Gestão de Matrículas

Contrato conceitual da API REST consumida pelo frontend Angular. Nomes de
campos em inglês (Princípio II); mensagens de erro (`message`) em português,
prontas para exibição direta ao usuário (Princípio I / R1). Formato exato
dos DTOs (records vs. classes, nullable annotations) fica a cargo das
tarefas de implementação — este documento fixa o formato observável pela
API, não a implementação interna.

Todas as respostas de erro de validação de negócio usam o mesmo formato:

```json
{ "message": "Este Pokémon já possui uma matrícula ativa." }
```

com HTTP `400 Bad Request` (ou `409 Conflict` para R1, dado que é um
conflito de estado) e `404 Not Found` para IDs inexistentes.

---

## Trainers

### `POST /api/trainers`

Cadastra um Treinador — FR-001, FR-002.

Request:
```json
{ "name": "Ash Ketchum", "email": "ash@example.com", "city": "Pallet Town" }
```

Response `201 Created`:
```json
{ "id": 1, "name": "Ash Ketchum", "email": "ash@example.com", "city": "Pallet Town" }
```

Erros: `409 Conflict` se `email` já estiver em uso — "Este e-mail já está
cadastrado para outro Treinador."

### `GET /api/trainers`

Lista Treinadores (para preencher seletor de destino na transferência e o
formulário de matrícula). Response `200 OK`: array do shape acima.

---

## Pokemons

### `POST /api/pokemons`

Cadastra um Pokémon — FR-003, FR-004, FR-022.

Request:
```json
{ "name": "Pikachu", "type": "Electric", "level": 25, "trainerId": 1 }
```

Response `201 Created`: mesmo shape com `id`.

Erros: `400 Bad Request` se `level` fora de 1-100, ou `type` fora da lista
fixa de 18 valores.

### `GET /api/pokemons`

Lista Pokémon (para formulário de matrícula e tela de transferência).
Response `200 OK`: array do shape acima.

### `POST /api/pokemons/{id}/transfer`

Transfere um Pokémon para outro Treinador — FR-015, R5.

Request:
```json
{ "newTrainerId": 2 }
```

Response `200 OK`:
```json
{
  "pokemon": { "id": 5, "name": "Pikachu", "type": "Electric", "level": 25, "trainerId": 2 },
  "closedEnrollmentId": 10,
  "newEnrollmentId": 14
}
```
(`closedEnrollmentId`/`newEnrollmentId` são `null` se o Pokémon não tinha
matrícula ativa no momento da transferência — cenário 4 de US4.)

Erros: `404 Not Found` se `newTrainerId` não existir; `400 Bad Request` se
`newTrainerId` for igual ao Treinador atual do Pokémon.

---

## Training Plans

### `GET /api/training-plans`

Lista os 3 planos fixos (para seletor no formulário de matrícula/upgrade).

Response `200 OK`:
```json
[
  { "id": 1, "name": "Ginásio Local", "monthlyPrice": 50.00, "description": "Treinos básicos" },
  { "id": 2, "name": "Liga Regional", "monthlyPrice": 120.00, "description": "Treinos intermediários + batalhas simuladas" },
  { "id": 3, "name": "Elite dos 4", "monthlyPrice": 300.00, "description": "Preparação completa para a Liga" }
]
```

---

## Enrollments

### `POST /api/enrollments`

Matricula um Pokémon em um plano — FR-005, FR-006, FR-007, US1.

Request:
```json
{ "pokemonId": 5, "trainingPlanId": 1 }
```
(`startDate` não é um campo do request — FR-018 — é definida pelo servidor.)

Response `201 Created`:
```json
{
  "id": 10,
  "pokemonId": 5,
  "trainingPlanId": 1,
  "startDate": "2026-07-17T14:30:00Z",
  "endDate": null,
  "monthlyPrice": 50.00,
  "status": "Active"
}
```
(`status` é um campo calculado — `Active` / `EndingSoon` / `Ended` — espelha
a derivação de FR-020 para o frontend não reimplementar a regra.)

Erros:
- `409 Conflict` se o Pokémon já tiver matrícula ativa — "Este Pokémon já
  possui uma matrícula ativa." (R1)
- `400 Bad Request` se o plano for "Elite dos 4" e `Pokemon.Level < 50` —
  "Nível mínimo de 50 é necessário para o plano Elite dos 4." (R3)

### `GET /api/enrollments?search={text}&status={Active|EndingSoon|Ended}`

Lista/busca/filtra matrículas — FR-016, FR-017, US3. `search` compara contra
nome do Pokémon ou do Treinador, case-insensitive e accent-insensitive.
Ambos os parâmetros são opcionais e combináveis.

Response `200 OK`: array de objetos no formato:
```json
{
  "id": 10,
  "pokemonName": "Pikachu",
  "trainerName": "Ash Ketchum",
  "trainingPlanName": "Ginásio Local",
  "startDate": "2026-07-17T14:30:00Z",
  "endDate": null,
  "monthlyPrice": 50.00,
  "status": "Active"
}
```

### `POST /api/enrollments/{id}/upgrade/preview`

Calcula (sem efeitos colaterais) o valor da primeira cobrança de um upgrade
— FR-009, FR-021, US2.

Request:
```json
{ "newTrainingPlanId": 2 }
```

Response `200 OK`:
```json
{
  "currentPlanCredit": 25.00,
  "newPlanProratedCost": 60.00,
  "firstChargeAmount": 35.00,
  "cycleEndDate": "2026-07-31T00:00:00Z",
  "daysRemainingInCycle": 15
}
```

Erros:
- `400 Bad Request` se `newTrainingPlanId` tiver `monthlyPrice` menor ou
  igual ao plano atual — "Downgrade de plano não é permitido." (R2)
- `400 Bad Request` se o novo plano for "Elite dos 4" e
  `Pokemon.Level < 50` — mesma mensagem de R3 acima.

### `POST /api/enrollments/{id}/upgrade/confirm`

Aplica o upgrade previamente calculado — FR-010.

Request: mesmo shape do preview (`{ "newTrainingPlanId": 2 }`) — o valor não
é reenviado pelo cliente; o servidor recalcula no momento da confirmação
para evitar inconsistência caso o preview tenha expirado.

Response `201 Created`:
```json
{
  "closedEnrollmentId": 10,
  "newEnrollment": {
    "id": 11, "pokemonId": 5, "trainingPlanId": 2,
    "startDate": "2026-07-17T14:30:00Z", "endDate": null,
    "monthlyPrice": 120.00, "status": "Active"
  },
  "firstChargeAmount": 35.00
}
```

Erros: mesmos do preview.

### `POST /api/enrollments/{id}/cancel`

Cancela uma matrícula ativa — FR-012, US4.

Response `200 OK`:
```json
{ "id": 10, "endDate": "2026-07-31T00:00:00Z", "status": "EndingSoon" }
```

Erros: `400 Bad Request` se a matrícula já estiver encerrada.
