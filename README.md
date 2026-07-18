# Centro de Treinamento Pokémon "Alto Nível"

Sistema de gestão de matrículas de Pokémon em planos de treinamento.

---

## Estrutura do Projeto

```
Desafio/
├── backend/PokemonTrainingCenter/   # API REST em .NET 8
├── frontend/                        # SPA em Angular 15
└── database/
    ├── schema.sql                   # Script de criação do banco
    └── consulta-mrr.sql             # Consulta MRR por plano
```

---

## Pré-requisitos

| Ferramenta         | Versão mínima |
|--------------------|---------------|
| .NET SDK           | 8.0           |
| SQL Server         | 2019 ou superior (LocalDB também funciona) |
| Node.js            | 14.20+        |
| Angular CLI        | 15.x          |

---

## 1. Banco de Dados

### Opção A — Migrations automáticas (recomendado)

A API aplica as migrations automaticamente ao iniciar. Basta garantir que o SQL Server está acessível e ajustar a connection string em `backend/PokemonTrainingCenter/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PokemonTrainingCenter;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Para SQL Server Express ou instância nomeada, use: `Server=localhost\SQLEXPRESS;...`
> Para LocalDB: `Server=(localdb)\\mssqllocaldb;Database=PokemonTrainingCenter;Trusted_Connection=True;`

### Opção B — Script manual

Execute `database/schema.sql` no SQL Server Management Studio ou via `sqlcmd`:

```bash
sqlcmd -S localhost -i database/schema.sql
```

---

## 2. Backend (API .NET 8)

```bash
cd backend/PokemonTrainingCenter
dotnet run
```

A API sobe em `http://localhost:5000`.  
O Swagger fica disponível em `http://localhost:5000/swagger`.

---

## 3. Frontend (Angular 15)

```bash
cd frontend
npm install
ng serve
```

Acesse `http://localhost:4200`.

> Se `ng` não for reconhecido: `npm install -g @angular/cli@15`

---

## Endpoints da API

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | /api/treinadores | Lista treinadores |
| POST | /api/treinadores | Cria treinador |
| PUT | /api/treinadores/{id} | Atualiza treinador |
| DELETE | /api/treinadores/{id} | Remove treinador |
| GET | /api/pokemons | Lista pokémons (filtro: `?treinadorId=`) |
| POST | /api/pokemons | Cria pokémon |
| PUT | /api/pokemons/{id} | Atualiza pokémon |
| PATCH | /api/pokemons/{id}/transferir | Transfere para outro treinador |
| GET | /api/matriculas | Lista matrículas (filtros: `?busca=&status=`) |
| POST | /api/matriculas | Cria matrícula |
| PATCH | /api/matriculas/{id}/cancelar | Cancela matrícula |
| GET | /api/matriculas/{id}/upgrade/preview | Calcula valor do upgrade (R2) |
| POST | /api/matriculas/{id}/upgrade | Executa o upgrade |

---

## Regras de Negócio Implementadas

### R1 — Matrícula única ativa
Um Pokémon não pode ter duas matrículas ativas ao mesmo tempo. Rejeitado na API com HTTP 422 e mensagem clara. O frontend exibe o erro de forma amigável (alerta vermelho).  
Reforçado também com índice único no banco (`UX_Matriculas_Pokemon_Ativa` filtrando `Status = 'Ativa'`).

### R2 — Upgrade com pro-rata
O endpoint `GET /api/matriculas/{id}/upgrade/preview?novoPlano=LigaRegional` retorna:
- `creditoPlanoAntigo`: valor proporcional aos dias não utilizados do plano atual
- `custoNovoPlanoDiasRestantes`: custo do novo plano pelos dias restantes
- `primeiraCobranca`: diferença a ser cobrada (nunca negativa)

O frontend exibe essa prévia antes de pedir confirmação. Downgrade retorna HTTP 422.

O ciclo é calculado a partir da `DataInicio` da matrícula (mês corrente de 30 dias variáveis via `AddMonths(1)`).

### R3 — Nível mínimo Elite dos 4
Pokémon com nível < 50 não pode ser matriculado nem receber upgrade para o plano Elite dos 4. Validado tanto na API quanto no frontend (o card do plano é desabilitado visualmente).

### R4 — Matrículas canceladas
Matrículas canceladas ficam registradas no histórico com `Status = 'Cancelada'`. Não entram no cálculo de MRR (`consulta-mrr.sql` filtra apenas `Status = 'Ativa'`). Não podem ser canceladas novamente.

### R5 — Transferência de Pokémon
O endpoint `PATCH /api/pokemons/{id}/transferir` muda o `TreinadorId`. As matrículas **permanecem vinculadas ao Pokémon** (não ao Treinador), então continuam ativas normalmente. A listagem de matrículas mostrará o novo treinador automaticamente, pois o nome é derivado da relação Pokémon → Treinador no momento da consulta.

---

## Decisões Técnicas

**Por que os planos estão como enum e não em tabela?**  
O enunciado define os três planos com valores fixos. Armazenar em tabela adicionaria complexidade sem ganho real para o escopo do desafio. Se os planos fossem dinâmicos, migraria para tabela.

**Por que `ValorMensal` é salvo na matrícula?**  
Mantém o histórico correto mesmo que o valor do plano mude no futuro. A primeira cobrança do upgrade (pro-rata) também é persistida.

**Migrations vs schema.sql manual**  
O projeto usa EF Core Migrations aplicadas automaticamente no startup (`db.Database.Migrate()`). O `schema.sql` está disponível para visualização e execução manual, mas não é o método principal.

**CORS**  
A API aceita qualquer origem em desenvolvimento para facilitar a integração local. Em produção, deve ser restrito.

---

## O que faria com mais tempo

- Testes unitários no `MatriculaService` (especialmente o cálculo de pro-rata)
- Paginação na listagem de matrículas
- Telas de CRUD de Treinadores e Pokémons no frontend
- Autenticação JWT
- Dashboard com cards de MRR por plano consumindo a consulta SQL
- Validação de e-mail no formulário de Treinador com feedback em tempo real

---

## Como usei IA (Claude)

Utilizei o Claude como par de desenvolvimento ao longo de todo o projeto:

- **Geração da estrutura inicial**: solicitei a criação dos models, DTOs, DbContext e controllers com as regras de negócio descritas no enunciado. Revisei cada arquivo antes de aceitar, corrigindo detalhes de nomenclatura e ajustando a lógica de pro-rata.
- **Cálculo de pro-rata (R2)**: descrevi o exemplo do enunciado e pedi a implementação; revisei a fórmula manualmente para garantir que `diasRestantes` nunca seja negativo e que `primeiraCobranca` seja ≥ 0.
- **Consulta SQL (MRR)**: pedi o esqueleto da query com `UNION ALL` para a linha de total; ajustei o `ORDER BY` com `CASE` para garantir a ordenação correta dos planos.
- **Frontend Angular**: solicitei os templates HTML e os estilos CSS; refinei a UX do modal de upgrade e o comportamento do card de plano desabilitado para Elite dos 4.
- **O que precisei corrigir**: compatibilidade de versões (EF Core 8 com .NET 8, Angular CLI 15 com Node 14), caminho de ferramentas (`dotnet-ef` global), e pequenos ajustes de tipagem nos componentes Angular.

Compreendo integralmente o código produzido e consigo explicar qualquer trecho ou modificá-lo ao vivo.
