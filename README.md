# Centro de Treinamento Pokémon "Alto Nível"

Sistema de gestão de matrículas de Pokémon em planos de treinamento mensais,
desenvolvido como desafio técnico. Composto por API REST em .NET, banco de
dados SQL Server e frontend em Angular.

O enunciado original do desafio está em [DESAFIO.md](./DESAFIO.md).

## Sumário
- [Instruções de execução](#instruções-de-execução)
  - [Pré-requisitos](#pré-requisitos)
  - [1. Banco de dados](#1-banco-de-dados)
  - [2. Backend (API)](#2-backend-api)
  - [3. Testes automatizados do backend](#3-testes-automatizados-do-backend)
  - [4. Frontend](#4-frontend)
  - [5. Consulta de MRR](#5-consulta-de-mrr)
- [Decisões técnicas e premissas](#decisões-técnicas-e-premissas)
- [Uso de IA](#uso-de-ia)
  - [Fluxo SDD](#fluxo-sdd)
  - [Revisões e correções](#revisões-e-correções)
- [O que eu faria diferente ou melhoraria com mais tempo](#o-que-eu-faria-diferente-ou-melhoraria-com-mais-tempo)

## Instruções de execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para o container do banco)
- [Node.js LTS](https://nodejs.org/) + Angular CLI (`npm install -g @angular/cli`)

### 1. Banco de dados

Na raiz do projeto:

```powershell
docker compose up -d
```

Sobe um único container com o SQL Server.

### 2. Backend (API)

```powershell
cd backend/src/PokemonTrainingCenter.Api
dotnet ef database update   # aplica as migrations e cria o schema no container
dotnet run
```

A API sobe em `http://localhost:5141` (o perfil de execução `http` é o primeiro do `launchSettings.json`, logo, é o usado por padrão).

### 3. Testes automatizados do backend

```powershell
cd backend
dotnet test
```

### 4. Frontend

```powershell
cd frontend
npm install
npm start
```

Acesse `http://localhost:4200`.

### 5. Consulta de MRR

Com o container do banco rodando, execute `database/consulta-mrr.sql` diretamente contra ele, usando qualquer cliente SQL Server (Azure Data Studio, SSMS, DBeaver, `sqlcmd`, extensão SQL Server do VS Code etc.), com os seguintes dados de conexão:

- **Host:** `localhost`
- **Porta:** `1433`
- **Usuário:** `sa`
- **Senha:** `PokemonTC!2026`
- **Banco:** `PokemonTrainingCenter`

## Decisões técnicas e premissas

**Status de matrícula (Ativa/Cancelada/Concluída):** o enunciado define os três status mas não especifica o que diferencia "Cancelada" de "Concluída". Optei por remover o atributo de status como campo persistido e substituí-lo por um único campo `EndDate` (nullable), comparado pelo **instante exato** (`EndDate >= DateTime.UtcNow`), não só pela data. O estado da matrícula passa a ser derivado:
- `EndDate = NULL` → matrícula ativa, sem encerramento agendado.
- `EndDate` no futuro (ou igual ao instante atual) → matrícula ativa, mas com encerramento agendado (cancelamento solicitado, ainda dentro do período pago).
- `EndDate` no passado → matrícula encerrada.

Cada operação grava um instante diferente em `EndDate`, conforme o efeito desejado: cancelamento grava o fim do dia (UTC) do ciclo pago vigente (o Pokémon mantém acesso durante todo esse dia), enquanto upgrade e transferência gravam o instante exato da operação (encerrando a matrícula anterior imediatamente, já que uma nova a substitui). Comparar por instante em vez de só por data evita um efeito colateral: sem isso, no mesmo dia em que uma matrícula é encerrada por upgrade ou transferência, ela continuaria aparecendo como "A encerrar" até a virada do dia, e a consulta de MRR contaria a matrícula antiga (já substituída) em dobro com a nova nesse mesmo dia.

O status ("Ativa", "A encerrar", "Encerrada") não vem de um campo salvo no banco — a API recalcula ele a partir de `EndDate` a cada resposta, e o frontend só traduz esse valor para o rótulo em português exibido na tela. Se o status fosse um campo próprio, toda operação que altera o ciclo de vida da matrícula (cancelamento, upgrade, transferência, ou a simples passagem do tempo) precisaria lembrar de atualizá-lo junto com `EndDate`, arriscando os dois ficarem dessincronizados. Derivando o status sempre de `EndDate`, essa inconsistência deixa de ser possível. A regra R1 (matrícula única ativa por Pokémon) e a consulta de MRR foram ajustadas para considerar "ativa" como `EndDate IS NULL OR EndDate >= agora`.

**R4 (matrículas canceladas nos cálculos e relatórios):** matrículas com `EndDate` no passado são excluídas do cálculo de MRR, mas o registro é mantido no banco (sem exclusão física) para preservar histórico.

**Armazenamento de datas em UTC + `DateTimeKind`:** `StartDate`/`EndDate` sempre foram gravados com `DateTime.UtcNow` (decisão desde a implementação original), o que é o comportamento correto — evita bugs de fuso horário e horário de verão. Só que o SQL Server (`datetime2`) não preserva `DateTimeKind`, então o EF Core devolvia `Kind=Unspecified` ao reler do banco; o `System.Text.Json` só inclui o sufixo `Z` (UTC) quando `Kind=Utc`, e sem ele o `DatePipe` do Angular interpretava o valor como se já fosse hora local do navegador — sem nenhuma conversão. Corrigido com um `ValueConverter` global em `AppDbContext.ConfigureConventions` que marca todo `DateTime` lido do banco como `Kind=Utc`.

**R5 (transferência de Pokémon entre Treinadores):** se o Pokémon tiver matrícula ativa no momento da transferência, o sistema encerra automaticamente essa matrícula sob o Treinador de origem (`EndDate` = data da transferência, sem estorno do valor já pago) e cria automaticamente uma nova matrícula no mesmo plano sob o Treinador de destino, com cobrança integral (sem pro-rata, por se tratar de um novo ciclo) a partir dessa data. 

Isso tem uma limitação conhecida: como a origem não recebe estorno e o destino paga o ciclo cheio a partir da data da transferência, o CT efetivamente "recebe" duas vezes pelos mesmos dias — uma vez da origem (valor não devolvido) e outra do destino (cobrança cheia, não proporcional). A correção fica descrita em "Melhorias futuras" (Motor de cobrança recorrente).

**Snapshot do Treinador na matrícula:** ao testar manualmente o fluxo de transferência (R5), encontrei um bug: `EnrollmentListItemResponse` derivava o nome do Treinador exibido navegando `Matrícula → Pokémon → Treinador`, ou seja, sempre o dono **atual** do Pokémon. Como a transferência muda `Pokemon.TrainerId`, toda matrícula daquele Pokémon — ativa ou já encerrada, de qualquer período — passava a exibir o novo Treinador, apagando o histórico de quem era dono durante cada uma. Corrigi adicionando `Enrollment.TrainerId` (FK para `Trainer`), preenchido no momento da criação de cada matrícula e nunca reescrito depois — um snapshot histórico, exatamente como já existia para `Enrollment.MonthlyPrice` (snapshot de preço). Upgrade preserva o `TrainerId` da matrícula anterior (o dono não muda); transferência grava o novo Treinador só na matrícula nova, mantendo a antiga com o Treinador de origem. Exigiu uma migration EF Core com backfill dos dados existentes a partir do `Pokemon.TrainerId` atual — única informação disponível para matrículas já criadas antes deste fix.

**R2 (duração do ciclo mensal no cálculo pro-rata):** o enunciado usa "ciclo de 30 dias" apenas como exemplo, não como regra fixa — o texto fala em "ciclo mensal" o tempo todo. Por isso, a duração do ciclo usada no pro-rata é sempre o número real de dias entre a data de início (ou o aniversário mensal mais recente) e a mesma data um mês depois. Para datas de início que não existem todo mês (29, 30 ou 31), o ciclo seguinte termina no último dia do mês em que esse dia não existir — ex.: matrícula iniciada em 31/01 tem ciclo terminando em 28/02 (ou 29/02 em ano bissexto). Essa é exatamente a semântica de `DateTime.AddMonths` do .NET, então a regra de negócio se traduz diretamente para código sem tratamento especial de calendário.

**Data de início da matrícula:** não é um campo preenchido manualmente no formulário. O sistema a define automaticamente como o momento da confirmação (a "data de contratação"), análogo ao instante em que os dados de pagamento seriam capturados em um sistema real de cobrança recorrente por cartão de crédito.

**Tipos de Pokémon:** o enunciado cita "Fogo, Água, Planta..." como exemplos, então fixei a lista completa dos 18 tipos oficiais da franquia (Normal, Fogo, Água, Planta, Elétrico, Gelo, Lutador, Venenoso, Terra, Voador, Psíquico, Inseto, Pedra, Fantasma, Dragão, Sombrio, Aço, Fada) como um conjunto fechado, em vez de um campo de texto livre.

**Arredondamento no cálculo pro-rata:** como o ciclo mensal pode ter 28 a 31 dias (ver decisão de R2 acima), a divisão usada no pro-rata pode gerar dízimas (ex.: dividir por 29). O resultado é sempre arredondado para duas casas decimais com arredondamento padrão (0,5 arredonda para cima), para o valor exibido e cobrado ficar sempre em centavos, como em qualquer sistema de pagamento real.

**Linha de Total Geral na consulta de MRR:** mesmo que nenhum plano tenha matrícula ativa, a consulta sempre retorna a linha de Total Geral (com R$ 0,00), em vez de um resultado vazio — evita que quem consome o relatório confunda "sem receita ativa no momento" com erro na consulta.

**Busca por nome (Pokémon/Treinador):** case-insensitive e accent-insensitive (buscar "agua" encontra "Água"), para não exigir que o usuário digite acentos corretamente.

**Bootstrap para estilização:** optei por Bootstrap (apenas CSS, sem o bundle de componentes) em vez de escrever CSS do zero ou usar uma biblioteca de componentes específica do Angular (como o Angular Material). Como o enunciado não avalia beleza visual mas avalia validações e experiência de uso, as classes prontas do Bootstrap (validação de formulário, alertas de erro, badges de status, tabelas) cobrem exatamente esses pontos sem o overhead de configuração de tema/módulos que o Angular Material exigiria.

**Execução do banco em Docker:** o SQL Server roda em um único container (`docker-compose.yml` na raiz do projeto), enquanto o backend e o frontend rodam nativos na máquina (`dotnet run` / `ng serve`). Preferi não containerizar os três serviços porque hot-reload dentro de container exige configuração extra (volumes, polling de arquivo) sem ganho para os critérios avaliados; um único container de banco já resolve o principal problema de portabilidade — minha ideia inicial era usar o SQL Server LocalDB, mas ele só existe no Windows.

**Telas de listagem de Treinadores e Pokémons:** o enunciado só exige listagem de Matrículas; adicionei listagens de Treinadores e Pokémons (sem busca/filtro, já que não são exigidos) para dar visibilidade ao estado do sistema sem depender do banco de dados diretamente, e para os 3 formulários terem uma página de destino natural ao concluir. O menu superior passou a linkar as 3 listagens; cada listagem ganhou um botão "+ Novo" para o formulário correspondente.

## Uso de IA

### Fluxo SDD

Utilizei o GitHub Spec Kit integrado ao Claude Code para conduzir o desenvolvimento com uma abordagem de Spec-Driven Development (SDD). O fluxo foi:

1. **Especificação**: a partir do enunciado original, gerei com o Claude Code (via `/speckit.constitution` e `/speckit.specify`) uma especificação formal das regras de negócio (R1-R5), incluindo cenários de teste derivados dos exemplos do enunciado (como o cálculo pro-rata do upgrade). Revisei e solicitei ajustes dessas specs. Em seguida, usei `/speckit.clarify` para varrer o spec em busca de ambiguidades remanescentes antes de seguir para o planejamento; a IA identificou 3 lacunas e eu indiquei as ações corretivas.

2. **Planejamento**: usei `/speckit.plan` e `/speckit.tasks` para definir a estrutura do projeto e quebrar o trabalho em tarefas menores.

3. **Implementação**: diferente da especificação e do planejamento (conduzidos com decisões discutidas e aprovadas passo a passo ao longo da conversa), a implementação em si foi feita de uma vez só: rodei `/speckit-implement` uma única vez, que executou as 58 tarefas de `tasks.md` de ponta a ponta.

4. **Edições pontuais**: para mudanças fora do fluxo completo dos comandos do Spec Kit, mantenho no `CLAUDE.md` do projeto a regra de que o Claude Code deve atualizar os artefatos do Spec Kit (spec.md, plan.md, tasks.md, contracts/api.md) antes de qualquer código sempre que a mudança afetar comportamento, e parar para minha revisão antes de implementar. A regra vale em qualquer sessão nova automaticamente — não depende de eu lembrar de repetir o pedido a cada prompt — e mantém o `spec.md` como fonte da verdade sobre o que o sistema faz. O mesmo arquivo também fixa as convenções de commit.

5. **Fluxo de trabalho paralelo:** utilizei o `/remote-control` do Claude Code para acompanhar e aprovar tarefas em execução mesmo longe do computador. Também mantive, em paralelo, uma sessão de chat separada com o Claude para discutir decisões de arquitetura, revisar premissas e planejar próximos passos antes de repassar instruções ao Claude Code — isso ajudou a economizar contexto na sessão de execução e a chegar a cada tarefa com a decisão já pensada, em vez de deixar a ferramenta decidir sozinha.

### Revisões e correções

**Change detection zoneless mal configurado:** a listagem de matrículas só aparecia depois de clicar no input do filtro, e os `<select>` de todos os formulários só mostravam as opções ao clicar duas vezes. O Angular CLI 22 usado neste projeto já roda sem a biblioteca `zone.js` — mas isso sozinho não torna o app "zoneless", só remove o mecanismo padrão que notifica o Angular quando algo muda (clique, resposta HTTP etc.), sem colocar nada no lugar. Sem `provideZonelessChangeDetection()` declarado, o Angular só re-renderiza nos poucos casos em que escuta um evento de template diretamente (como um `(click)`) — nunca depois de um `.subscribe()` de HTTP terminar sozinho, que era exatamente o sintoma. Corrigido declarando `provideZonelessChangeDetection()` no `app.config.ts` e migrando o estado assíncrono de listagens/formulários para `signal()`, que esse scheduler passa a rastrear nativamente.

**Formulários sem feedback de sucesso:** os 3 formulários de cadastro eram enviados sem nenhum feedback de sucesso aparecer. Corrigido inserindo navegação via `Router` para a listagem correspondente, com uma mensagem de confirmação passada via router state e exibida na listagem de destino.

**Validação de e-mail com TLD obrigatório:** o `Validators.email` embutido do Angular aceita endereços sem TLD (ex.: `nome@dominio`, sem ".com"). Adicionei um validador customizado (regex exigindo um domínio com TLD de 2+ caracteres) no frontend, e a mesma validação no backend (`TrainersController`) como defesa em profundidade, já que a validação client-side pode ser contornada por quem chama a API diretamente.

**Repository pattern para acesso a dados:** a versão original acessava `AppDbContext` diretamente em `EnrollmentService` e nos 4 controllers, misturando lógica de negócio/HTTP com acesso a dados. Introduzi interfaces de repository (`ITrainerRepository`, `IPokemonRepository`, `ITrainingPlanRepository`, `IEnrollmentRepository`) e `IUnitOfWork` no projeto `Domain`, com as implementações concretas (usando EF Core) no `Infrastructure`. Isso teve um efeito colateral positivo: o `AppDbContext` pôde voltar para `Infrastructure` (de onde havia sido movido numa revisão anterior só para evitar uma referência circular, já que `EnrollmentService` no `Domain` precisava consultar o banco) — com as interfaces fazendo essa ponte, o `Domain` deixou de depender do EF Core inteiramente. `AppDbContext` passou a implementar `IUnitOfWork` diretamente (já expõe `SaveChangesAsync(CancellationToken)` com a assinatura exata), evitando uma classe wrapper extra. As interfaces expõem só os métodos que o código já usava (ex.: `HasOpenOrActiveAsync`, `GetAllWithDetailsAsync`), sem CRUD genérico especulativo. Mudança arquitetural pura — nenhuma regra de negócio (R1-R5) foi afetada, e a validação incluiu tanto os testes automatizados quanto uma verificação manual via `curl` contra a API e o banco reais, cobrindo cada endpoint que passou a depender de um repository.

**Clarificações:** durante o `/speckit-specify`, a IA gerou um primeiro rascunho do spec com pontos de ambiguidade sinalizados (`[NEEDS CLARIFICATION]`) e opções sugeridas para cada um — mas nenhuma das opções cobria exatamente o que eu queria, então respondi com decisões próprias em vez de escolher entre A/B/C:
   - Para a transferência de Pokémon (R5), o primeiro rascunho da IA exigia que o Treinador de destino criasse manualmente uma nova matrícula após a transferência. Percebi que isso deixava o Pokémon sem treinamento ativo por tempo indeterminado — inconsistente com o pedido do enunciado de que "as matrículas se comportem de forma coerente" — e pedi que a recriação da matrícula fosse automática.
   - Para o cálculo pro-rata (R2), a IA havia deixado a duração do ciclo mensal como ambígua sem propor uma solução técnica; ao perguntar especificamente sobre matrícula iniciada no dia 31, pedi que a própria IA propusesse a regra (ciclo = mesma data um mês depois, com ajuste para o último dia do mês quando esse dia não existir), que revisei e aprovei.
   - Também corrigi dois gaps que a IA não havia coberto no rascunho inicial: o tipo do Pokémon estava especificado como campo livre (só com exemplos "Fogo, Água, Planta..."), quando deveria ser uma lista fechada dos 18 tipos oficiais; e a data de início da matrícula estava como campo editável no formulário, quando faz mais sentido ser definida automaticamente no momento da contratação, como em um sistema real de cobrança recorrente.

**Revisões do `plan.md`:** 
   - a IA havia decidido não usar nenhuma biblioteca de CSS ("HTML/CSS simples"); solicitei Bootstrap para facilitar. 
   - a IA havia escolhido SQL Server LocalDB como banco padrão de desenvolvimento; ao sugerir Docker, ela apontou que LocalDB só funciona no Windows — uma limitação de portabilidade que ela não tinha sinalizado antes — e propôs um único container de banco via Docker Compose, mantendo backend e frontend nativos para não perder o hot-reload.

## O que eu faria diferente ou melhoraria com mais tempo

- **Fluxo de transferência com aceite:** hoje a transferência de Pokémon entre Treinadores é direta e imediata. Com mais tempo, implementaria um fluxo de solicitação/aceite (com uma tabela dedicada de transferências, status "aguardando aceite"/"concluída", e uma tela extra de "Transferências" listando solicitações enviadas e recebidas), mais coerente com um produto real onde ambas as partes precisam concordar com a mudança de posse.
- **Desfazer uma transferência solicitada:** não é possível cancelar uma transferência já solicitada antes dela se consolidar — uma adição natural do fluxo de aceite acima, quando ele existir.
- **"Descancelar" uma matrícula (reverter um cancelamento ainda não consolidado):** hoje, uma vez cancelada (status "Ativa a encerrar"), não há nenhuma saída antes do fim do ciclo — não é possível reverter o cancelamento. Ou seja: o único caminho hoje é esperar o ciclo terminar. Isso tem um efeito colateral ruim: enquanto isso, R1 continua bloqueando qualquer nova matrícula para aquele Pokémon, mesmo que o Treinador mude de ideia no mesmo dia do cancelamento. Um endpoint de "descancelar" (voltar `EndDate` para `null`) resolveria isso diretamente.
- **Notificações por email:** eventos como confirmação de nova matrícula, aviso de próxima cobrança, solicitação de transferência recebida e confirmação de cancelamento poderiam ser comunicados por email ao Treinador. Para ações sensíveis (transferência de Pokémon, cancelamento de plano), o email incluiria um aviso do tipo "não foi você? troque sua senha", como camada adicional de segurança contra acesso indevido à conta.
- **Autenticação, autorização e confirmação de conta:** o desafio não pede login, então não foi implementado controle de acesso por Treinador. Em um cenário real, cada Treinador só veria/gerenciaria seus próprios Pokémon e matrículas, com confirmação de email no cadastro.
- **Motor de cobrança recorrente:** o sistema atual não simula cobrança automática mensal. Um motor de recorrência (com histórico de cobranças por matrícula) resolveria de forma completa a cobrança dupla na transferência (ver limitação conhecida em "Decisões técnicas"): `Enrollment` ganharia um campo `NextBillingDate` (`DateTime`). Numa matrícula comum, `NextBillingDate` nasceria igual a `StartDate`; numa transferência, herdaria o fim do ciclo já pago pela origem, adiando a primeira cobrança real do destino para quando esse ciclo terminaria de qualquer forma. O motor seria responsável tanto por gerar cada cobrança quanto por avançar `NextBillingDate` um ciclo por vez — sem essa rotina, o campo ficaria estático depois de escrito uma única vez, por isso não foi adicionado agora sem o motor que o mantém.
- **Testes de integração end-to-end:** os testes unitários cobrem a lógica de negócio isoladamente (cálculo de upgrade, validações), mas testes de integração cobrindo o fluxo completo via API (e idealmente E2E no frontend) dariam mais confiança para regressões futuras.
- **Cidade do Treinador como seletor em cascata (País → Estado → Cidade):** hoje "cidade de origem" é um campo de texto livre, o que aceita qualquer valor (inclusive inválido ou digitado de formas diferentes para o mesmo lugar). Trocar por uma lista suspensa condicional — escolhe o Estado, e as opções de Cidade mudam de acordo — eliminaria esse problema e facilitaria filtros por região caso o sistema ganhe um CRUD completo de Treinadores no futuro. Não implementado agora porque exigiria uma base de dados de estados/cidades como nova dependência, além do formulário em si.
- **Fuso horário fixo em UTC no cancelamento:** ao cancelar uma matrícula, o "fim do dia" gravado em `EndDate` é o fim do dia *calendário UTC*, não o fim do dia no fuso horário local de quem está usando o sistema (ex.: Brasília, UTC-3). Na prática, isso encerra o acesso 3h antes da meia-noite no horário de Brasília. Resolver isso corretamente exigiria um conceito de fuso horário de negócio configurável (ou por Treinador), que ficou fora do escopo desta revisão — hoje o sistema só converte datas para hora local na exibição (frontend).
- **Confirmação de sucesso com toast/snackbar:** hoje a confirmação de sucesso após cadastro é um alerta Bootstrap fixo no topo da listagem de destino, sem auto-dismiss. Um componente de notificação temporário (toast) seria mais consistente com o padrão de UX de formulários que redirecionam após salvar.
- **Transferência de Pokémon como ação na linha da listagem:** hoje `pokemon-transfer` é uma tela própria, com seu próprio `<select>` de Pokémon — o usuário escolhe o Pokémon de novo, mesmo já estando na listagem de Pokémons olhando pra ele. Um botão "Transferir" na linha de cada Pokémon (mesmo padrão de ação por linha já usado em `enrollments-list` para Upgrade/Cancelar) pré-selecionaria o Pokémon e pouparia esse passo redundante.
- **Commits:** Teria sido melhor iniciar o projeto com a orientação atual do `CLAUDE.md` para commits, o que dividiria o commit da implementação inicial em fases menores e aglutinaria commits de revisões intermediárias que estavam num contexto de orientação para pequenos commits frequentes. A orientação atual é o equilíbrio.