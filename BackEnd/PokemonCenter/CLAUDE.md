# CLAUDE.md — Backend (API)

Este arquivo orienta o Claude Code no escopo do backend deste projeto.

## Contexto do projeto

API REST para o "Centro de Treinamento Pokémon Alto Nível" — sistema de gestão de matrículas de Pokémon em planos de treinamento mensais. Este é um desafio técnico para vaga de desenvolvedor(a) júnior. O enunciado original e completo do desafio está em `Docs/desafio-original.md`.

## Stack

- .NET 10 com **Minimal APIs** — não usar Controllers/MVC tradicional.
- Entity Framework Core, abordagem **database-first**: o schema é definido em `Database/schema.sql` e as entidades em `Models/` são geradas via `dotnet ef dbcontext scaffold`.
- SQL Server rodando via Docker (docker-compose na raiz do repo).

## Estrutura de pastas esperada

```
BackEnd/
├── Program.cs              # composição da app, mapeamento dos endpoints
├── Endpoints/               # um arquivo por grupo de recurso (TreinadorEndpoints.cs, PokemonEndpoints.cs, MatriculaEndpoints.cs)
├── Models/                  # GERADO por scaffold — não editar manualmente
├── DTOs/                    # request/response, nunca expor entidades do Models/ direto na API
└── Services/                # regras de negócio (R1-R5), separadas dos endpoints
```

Cada grupo de endpoints deve usar `MapGroup` para organizar rotas (ex.: `app.MapGroup("/api/matriculas")`), mantendo `Program.cs` enxuto — ele só deve compor os grupos, não conter lógica.

## Regras de negócio (resumo — ver desafio-original.md para o texto completo)

- **R1**: um Pokémon não pode ter duas matrículas com status `Ativa` simultâneas. Já reforçado no banco via índice único filtrado (`UX_Matricula_PokemonAtiva`), mas a API também deve validar e retornar erro 400 claro antes de tentar o INSERT.
- **R2**: upgrade de plano recalcula a primeira cobrança pro-rata. Fórmula: `credito = valorPlanoAntigo * (diasRestantes / diasCiclo)`; `custoNovo = valorPlanoNovo * (diasRestantes / diasCiclo)`; `primeiraCobranca = custoNovo - credito`. Exemplo de referência (usar em testes): Ginásio Local (R$50) → Liga Regional (R$120) no dia 16 de um ciclo de 30 dias = R$35 de primeira cobrança. Downgrade deve ser rejeitado (comparar `valorMensal` dos planos).
- **R3**: nível mínimo 50 para matricular ou fazer upgrade para o plano "Elite dos 4". Validar tanto na criação quanto no upgrade.
- **R4**: matrículas com status `Cancelada` devem ser excluídas de cálculos de MRR e relatórios ativos.
- **R5**: transferência de Pokémon entre treinadores só atualiza `treinadorId` em `Pokemon`; não deve alterar cobrança nem status de matrículas existentes (ver decisão documentada no README raiz).

## Regras do que NÃO fazer

- Não rodar `dotnet ef migrations add` nem `dotnet ef database update` — este projeto é database-first, schema vem do `.sql`, não de migrations.
- Não editar arquivos dentro de `Models/` manualmente — se o schema precisar mudar, altere `Database/schema.sql` e regenere via scaffold.
- Não implementar autenticação, login ou JWT — fora de escopo (painel administrativo interno, sem simulação de usuário final).
- Não usar Controllers/MVC — o padrão deste projeto é Minimal APIs.

## Convenções

- Nomes de variáveis, classes e comentários de código em português, consistente com o domínio do desafio (Treinador, Pokemon, Matricula).
- DTOs de resposta nunca devem expor a entidade do EF diretamente (evitar problemas de serialização circular e vazamento de detalhes internos).
- Erros de validação de negócio (R1, R3, downgrade em R2) devem retornar `400 Bad Request` com mensagem clara em português, pronta para exibição no frontend.
