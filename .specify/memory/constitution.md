<!--
Sync Impact Report
==================
Version change: TEMPLATE → 1.0.0 (initial ratification)
Modified principles: n/a (first concrete version)
Added sections:
  - Core Principles: I. Corretude das Regras de Negócio (NÃO NEGOCIÁVEL),
    II. Convenção Bilíngue de Idioma, III. Documentação de Decisões e Premissas,
    IV. Simplicidade e Escopo Mínimo, V. Transparência no Uso de IA
  - Stack Técnica e Estrutura do Projeto
  - Fluxo de Desenvolvimento e Entrega
  - Governance
Removed sections: none (all placeholders resolved)
Templates requiring updates:
  - ✅ .specify/templates/plan-template.md (Constitution Check gate is generic/dynamic — no edits needed)
  - ✅ .specify/templates/spec-template.md (Assumptions section already supports R4/R5-style decisions — no edits needed)
  - ✅ .specify/templates/tasks-template.md (test tasks remain optional per template; Principle I makes them mandatory for R1-R3 — no structural edit needed, enforced at planning time)
  - ✅ README.md (Uso de IA section already aligned with Principle V; no edit needed)
  - ✅ CLAUDE.md (language table is the source of truth for Principle II; referenced, not duplicated)
Follow-up TODOs: none
-->

# Centro de Treinamento Pokémon "Alto Nível" Constitution

## Core Principles

### I. Corretude das Regras de Negócio (NÃO NEGOCIÁVEL)

As regras R1 (matrícula única ativa), R2 (upgrade com cálculo pro-rata) e R3
(nível mínimo para Elite dos 4) são o critério de avaliação mais pesado do
desafio e DEVEM ser implementadas corretamente, incluindo casos de borda
(ciclo com 0 ou 30 dias restantes, upgrade no mesmo dia, downgrade rejeitado,
nível exatamente 50). Toda lógica que implemente R1, R2 ou R3 DEVE ter testes
automatizados que cubram o caminho feliz e os casos de borda; o teste de R2
DEVE validar o exemplo numérico do próprio enunciado (dia 16 de um ciclo de
30 dias → primeira cobrança de R$ 35,00). Mensagens de erro rejeitando uma
matrícula duplicada ou um downgrade DEVEM ser claras o suficiente para serem
exibidas ao usuário final sem reescrita adicional pelo frontend.

**Racional**: o próprio enunciado (DESAFIO.md) declara que a corretude de
R1-R3, incluindo casos de borda, é o primeiro item avaliado, e que uma sessão
de conversa técnica exigirá explicar e modificar esse código ao vivo — código
sem teste automatizado é código que não se pode defender com confiança nessa
sessão.

### II. Convenção Bilíngue de Idioma

Todo conteúdo gerado neste projeto DEVE seguir a tabela de idioma definida em
CLAUDE.md: identificadores de código (variáveis, classes, métodos,
propriedades, tabelas), logs e exceptions internas, XML doc/JSDoc, e nomes de
commit/branch/arquivo/projeto são em inglês; comentários de lógica de negócio,
mensagens de erro exibidas ao usuário final, README.md e artefatos do Spec Kit
(constitution.md, spec.md, plan.md, tasks.md) são em português. Esta
constituição é, ela própria, um artefato do Spec Kit e por isso é redigida em
português. Em caso de dúvida sobre uma categoria não coberta pela tabela,
DEVE-SE perguntar antes de assumir, em vez de adivinhar.

**Racional**: o projeto é uma entrega para avaliação técnica em uma empresa
brasileira; consistência de idioma entre código (inglês, padrão de mercado) e
comunicação com usuário/documentação (português) evita ambiguidade tanto para
quem revisa o código quanto para quem usa a aplicação.

### III. Documentação de Decisões e Premissas

Regras ambíguas ou incompletas do enunciado — explicitamente R4 (matrículas
canceladas em cálculos e relatórios) e R5 (transferência de Pokémon entre
Treinadores) — DEVEM ser resolvidas de uma das duas formas: (a) esclarecidas
antes da implementação via pergunta ao contato técnico do desafio, ou (b)
decididas unilateralmente e documentadas explicitamente no README, na seção
"Decisões técnicas e premissas". A seção `Assumptions` do spec.md gerado pelo
Spec Kit DEVE registrar essas mesmas premissas antes de virarem tarefas de
implementação. Nenhuma premissa sobre R4/R5 pode ficar implícita apenas no
código.

**Racional**: o próprio enunciado diz que ambas as atitudes (perguntar ou
decidir e documentar) são bem-vindas, e que o que é avaliado é como o
candidato lida com a ambiguidade — uma decisão não documentada não pode ser
avaliada.

### IV. Simplicidade e Escopo Mínimo

A estimativa do desafio é de 4 a 8 horas de trabalho. Implementação DEVE se
limitar ao que R1-R5, à consulta de MRR e às telas mínimas de Angular exigem;
abstrações, camadas extras, ou infraestrutura (ex.: filas, cache, microsserviços)
não solicitadas pelo enunciado NÃO DEVEM ser adicionadas. Beleza visual do
frontend é explicitamente fora do critério de avaliação — organização de
componentes, validações e experiência básica têm prioridade sobre polimento
visual.

**Racional**: o enunciado avalia organização e corretude, não abrangência;
tempo gasto em escopo não pedido é tempo tirado da corretude de R1-R3, que
pesa mais na avaliação.

### V. Transparência no Uso de IA

Toda geração de código, spec, plano ou tarefa assistida por IA (incluindo o
próprio uso do Spec Kit com Claude Code) DEVE ser registrada no README, seção
"Uso de IA": o que foi pedido, o que foi aproveitado, e o que precisou ser
corrigido ou reescrito manualmente. Lógica de negócio crítica (cálculo
pro-rata de R2, validação de matrícula única de R1, nível mínimo de R3) DEVE
ser revisada linha a linha pelo candidato antes de ser considerada concluída
— o candidato DEVE ser capaz de explicar e modificar esse código ao vivo na
sessão de conversa técnica.

**Racional**: o enunciado permite e incentiva uso de IA, mas condiciona a
avaliação à compreensão real do código pelo candidato, verificada em sessão
ao vivo; documentar o uso da IA é parte explícita dos requisitos do README.

## Stack Técnica e Estrutura do Projeto

O projeto é organizado em três diretórios na raiz: `/backend` (API REST em
.NET), `/frontend` (Angular) e `/database` (scripts SQL Server). O arquivo
`database/schema.sql` contém a modelagem das tabelas. O arquivo
`database/consulta-mrr.sql` DEVE ser escrito em SQL puro, sem ORM, e retornar
a Receita Mensal Recorrente agrupada por plano, considerando apenas matrículas
ativas, com uma linha de total geral ao final. Entidades mínimas: Treinador
(nome, e-mail único, cidade), Pokémon (nome, tipo, nível 1-100, Treinador
dono), Matrícula (Pokémon, Plano, data de início, status, valor mensal).
Planos de treinamento (Ginásio Local R$ 50, Liga Regional R$ 120, Elite dos 4
R$ 300) são um conjunto fixo conhecido em tempo de design, não exigem CRUD de
administração salvo indicação contrária do usuário.

## Fluxo de Desenvolvimento e Entrega

O desenvolvimento segue Spec-Driven Development via Spec Kit: constitution →
specify → clarify (quando necessário) → plan → tasks → implement. A entrega
final é feita por Pull Request contendo o nome completo do candidato. O README
raiz é obrigatório e DEVE conter, no mínimo: instruções de execução (backend,
frontend, scripts de banco), decisões técnicas e premissas assumidas
(especialmente R4 e R5, ver Princípio III), o que seria feito diferente ou
melhorado com mais tempo, e como a IA foi utilizada (ver Princípio V). O
projeto DEVE rodar localmente seguindo apenas as instruções do README.

## Governance

Esta constituição tem precedência sobre qualquer outra prática ou convenção
adotada informalmente durante o desenvolvimento. Toda mudança de escopo ou
decisão de arquitetura que viole um dos Princípios Fundamentais DEVE ser
justificada explicitamente na seção "Complexity Tracking" do plan.md gerado
pelo `/speckit-plan`, ou rejeitada. Emendas a esta constituição seguem
versionamento semântico (MAJOR.MINOR.PATCH): mudanças que removem ou
redefinem um princípio de forma incompatível são MAJOR; adição de novo
princípio ou seção é MINOR; esclarecimentos de redação são PATCH. Ao emendar,
propague o impacto para `.specify/templates/plan-template.md`,
`.specify/templates/spec-template.md` e `.specify/templates/tasks-template.md`
quando a mudança afetar seus gates ou estrutura. CLAUDE.md continua sendo a
fonte de verdade para a tabela de convenção de idioma referenciada no
Princípio II; em caso de conflito entre este documento e CLAUDE.md, CLAUDE.md
prevalece e esta constituição DEVE ser emendada na sequência para refletir a
divergência.

**Version**: 1.0.0 | **Ratified**: 2026-07-17 | **Last Amended**: 2026-07-17
