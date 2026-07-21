# Research: Gestão de Matrículas do Centro de Treinamento Pokémon

Nenhum item do Technical Context ficou marcado como `NEEDS CLARIFICATION` —
a constituição do projeto já fixa a stack (.NET/SQL Server/Angular) e o
`spec.md` já resolveu as ambiguidades de negócio via `/speckit-clarify`. Este
documento registra as decisões técnicas que ainda precisavam de uma escolha
concreta para a implementação, com racional e alternativas consideradas.

## 1. Versão do .NET

**Decision**: .NET 8 (LTS).

**Rationale**: o enunciado e a constituição exigem .NET mas não fixam
versão. .NET 8 é a LTS mais amplamente adotada e estável disponível,
reduzindo risco de incompatibilidade de tooling na máquina de quem avalia o
desafio. Se o ambiente de avaliação tiver uma LTS mais nova instalada, o
`TargetFramework` do `.csproj` pode ser atualizado trivialmente.

**Alternatives considered**: versão STS mais recente — rejeitada por ter
janela de suporte mais curta sem trazer nenhum recurso necessário para este
escopo.

## 2. ORM e origem do `database/schema.sql`

**Decision**: Entity Framework Core 8 (Code-First) como ORM da aplicação;
`database/schema.sql` é gerado via `dotnet ef migrations script` a partir da
migration inicial do EF Core, e commitado como artefato estático.

**Rationale**: escrever o modelo de dados em C# (Code-First) é mais rápido
dentro do orçamento de 4-8h (Princípio IV da constituição) do que manter um
`schema.sql` escrito à mão em paralelo ao modelo do EF Core — duas fontes de
verdade divergiriam com o tempo. Gerar o script a partir da migration
garante que o `schema.sql` entregue sempre reflete exatamente o que o EF
Core aplicaria, satisfazendo o entregável pedido pelo enunciado ("script de
criação") sem duplicar trabalho. Versionamento formal de migrations
(Flyway/EF Migrations History) fica registrado como melhoria futura no
README, já que não é exigido para esta entrega pontual.

**Alternatives considered**: (a) Dapper + `schema.sql` escrito à mão —
rejeitado por exigir mapeamento manual de todas as entidades sem ganho real
para o escopo; (b) manter `schema.sql` e o modelo do EF Core como fontes
independentes — rejeitado por risco de divergência silenciosa.

## 3. `database/consulta-mrr.sql`

**Decision**: T-SQL puro, escrito à mão, independente do EF Core, usando
`CASE WHEN EndDate IS NULL OR EndDate >= CAST(GETDATE() AS date)` para
filtrar matrículas ativas, `GROUP BY` plano com `GROUPING SETS` (ou `UNION
ALL` com uma consulta agregada sem `GROUP BY`) para a linha de Total Geral.

**Rationale**: exigência explícita do enunciado e do Princípio "Stack
Técnica" da constituição — este é o único artefato que não pode passar pelo
ORM. `GROUPING SETS` (ou `UNION ALL`) é a forma padrão em T-SQL de produzir
subtotais por grupo mais uma linha de total geral em uma única consulta,
sem duas queries separadas.

**Alternatives considered**: gerar o relatório via LINQ/EF Core e só
"exportar" o SQL gerado — rejeitado por violar diretamente a exigência de
"SQL puro, sem ORM".

## 4. Cálculo do ciclo mensal e arredondamento (R2)

**Decision**: usar `DateTime.AddMonths(1)` a partir da data de início (ou do
aniversário mensal mais recente) para achar o fim do ciclo vigente — o
próprio .NET já clampa automaticamente para o último dia do mês quando o
dia de início não existe no mês seguinte (ex.: 31/01 + 1 mês = 28/02 ou
29/02), que é exatamente a regra definida no FR-021. Todos os valores
monetários usam `decimal` (nunca `double`/`float`), e o resultado final do
pro-rata é arredondado com
`Math.Round(valor, 2, MidpointRounding.AwayFromZero)`, conforme a
clarificação registrada no spec (0,5 arredonda para cima).

**Rationale**: `DateTime.AddMonths` já implementa exatamente a semântica de
"aniversário mensal com clamp" que o FR-021 pede, sem precisar de lógica de
calendário customizada — decisão de baixo risco e fácil de testar. `decimal`
é o tipo padrão do .NET para valores monetários porque evita os erros de
arredondamento binário de `double`.

**Alternatives considered**: fixar o ciclo em 30 dias — rejeitado
explicitamente durante o `/speckit-clarify` (FR-021); usar `double` para os
cálculos — rejeitado por risco de imprecisão em valores monetários.

## 5. Garantia de matrícula única ativa sob concorrência (R1)

**Decision**: além da validação em nível de aplicação (verificar se já
existe uma matrícula com `EndDate IS NULL OR EndDate >= hoje` antes de criar
uma nova), adicionar um índice único filtrado no SQL Server:
`CREATE UNIQUE INDEX UX_Enrollment_PokemonId_Open ON Enrollment(PokemonId) WHERE EndDate IS NULL`.

**Rationale**: a validação em nível de aplicação sozinha tem uma janela de
corrida entre o `SELECT` de verificação e o `INSERT` sob requisições
concorrentes. Como R1 é a regra de maior peso na avaliação (Princípio I,
NÃO NEGOCIÁVEL), vale a defesa em profundidade de um índice único no banco
para o caso mais comum (matrícula sem encerramento agendado). O caso de uma
matrícula com `EndDate` futuro (ativa mas "a encerrar") já é coberto pela
checagem em nível de aplicação, pois nesse caso o valor de `EndDate` já foi
definido de forma síncrona por uma operação anterior (cancelamento/upgrade),
não por uma condição de corrida na criação.

**Alternatives considered**: apenas checagem em nível de aplicação —
rejeitada por não fechar a janela de corrida em um requisito não-negociável;
lock pessimista/transação serializável em toda a tabela — rejeitado por
complexidade desproporcional ao escopo (Princípio IV).

## 6. Frontend: Bootstrap (CSS) sem biblioteca de componentes Angular

**Decision**: Angular com Reactive Forms + Bootstrap (apenas CSS, via
`npm install bootstrap`, sem o bundle JS de componentes salvo o mínimo
necessário para elementos simples). Sem Angular Material ou similar.

**Rationale**: diferente de Angular Material — que exige módulos
Angular-específicos, configuração de tema e uma superfície de aprendizado
própria —, o Bootstrap é essencialmente CSS puro (classes utilitárias),
então não conflita com o Princípio IV (Simplicidade). Ele acelera
diretamente os pontos que **são** avaliados pelo enunciado ("validações e
experiência básica de uso"): classes prontas como `.is-invalid` +
`.invalid-feedback` para validação de formulário, `.alert-danger` para
mensagens de erro amigáveis (R1, R2, R3), `.badge` para as tags de status
(Ativa/A encerrar/Encerrada) e `.table` para a listagem de matrículas (US3).
O tempo economizado em CSS manual reverte para a lógica de negócio, que é o
que pesa mais na nota.

**Alternatives considered**: Angular Material — rejeitado no research
original por trazer módulos/tema Angular-específicos sem necessidade;
CSS 100% manual — reavaliado e trocado por Bootstrap após discussão, porque
o custo de configurar Bootstrap é menor do que escrever à mão os mesmos
padrões de validação/alerta que ele já resolve prontos.

## 7. Fluxo de upgrade: preview antes de confirmar

**Decision**: dois endpoints separados — um de preview (sem efeitos
colaterais, calcula e retorna o valor da primeira cobrança) e um de
confirmação (aplica a transição definida em FR-010).

**Rationale**: FR-009 exige que o valor calculado seja exposto ao usuário
*antes* da confirmação; separar preview de confirmação em duas chamadas HTTP
distintas é a tradução direta dessa exigência para uma API REST, e evita
que o cálculo tenha efeito colateral acidental se o usuário decidir não
confirmar.

**Alternatives considered**: um único endpoint que sempre aplica a mudança e
retorna o valor calculado — rejeitado por não permitir ao usuário desistir
depois de ver o valor, o que o FR-009/US2 exige implicitamente ("antes de
confirmar").

## 8. Execução local do SQL Server e dos demais serviços

**Decision**: SQL Server roda em um container Docker único, orquestrado por
um `docker-compose.yml` na raiz do repositório (`docker compose up db`).
Backend (`dotnet run`/`dotnet watch`) e frontend (`ng serve`) rodam nativos
na máquina, fora de container.

**Rationale**: LocalDB (decisão original) só existe no Windows — se quem
avaliar o desafio estiver em Mac/Linux, a instrução de setup simplesmente
não funciona. Um único container de banco via Docker Compose é reprodutível
em qualquer sistema operacional com uma linha de comando, sem exigir que o
avaliador instale SQL Server. Optou-se por **não** containerizar também o
backend e o frontend porque hot-reload (`dotnet watch`, `ng serve`) dentro
de container exige configuração extra (volumes montados, polling de
arquivo) que consome tempo de setup sem melhorar nenhum critério avaliado —
o ganho de portabilidade do banco já resolve a maior fonte de fricção
("instalar SQL Server"), enquanto rodar backend/frontend nativos mantém o
ciclo de desenvolvimento rápido dentro do orçamento de 4-8h (Princípio IV).

**Alternatives considered**: LocalDB nativo (decisão original) — trocado
por não ser multiplataforma; containerizar os três serviços — considerado e
rejeitado por custo de setup (Dockerfiles + networking + hot-reload) acima
do benefício para este escopo pontual; pode virar melhoria futura se o
projeto crescer além de uma entrega única.
