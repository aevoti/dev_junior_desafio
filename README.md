# Desafio Técnico — Desenvolvedor(a) Junior

Desenvolvido por Lívia Helena Rosa Scopel

Neste documento, vou falar um pouco sobre o que foi implementado, algumas regras de negócio escolhidas, um panorama geral do código e como rodar a aplicação.

## Backend

Foram implementadas 3 entidades: Pokemon, Trainer e Registration. Essas são também as tabelas que temos no banco de dados, mas vou falar mais sobre isso adiante.

Além disso, foram implementados 3 controladores, cada um correspondente à uma entidade. 
PokemonController: rotas de `POST`, `GET` e `PATCH`
TrainerController: rotas `POST` e `GET`
RegistrationController: rotas `POST`(criação e upgrade), `GET` e `PUT`

Vamos destrinchar cada rota


### PokemonController

`GET /api/pokemon (getAll):` Retorna a lista de todos os Pokémons cadastrados no banco.

`POST /api/pokemon (create):` Realiza o cadastro de um novo Pokémon. Valida a obrigatoriedade dos campos, limites de nível (1 a 100) e a existência do treinador no banco antes da inserção.

`PATCH /api/pokemon/{id}/replace-trainer:` Atualiza o trainer_id do Pokémon, realizando a transferência de dono. Valida se o Pokémon e  o novo treinador existem no banco antes de executar o update.

### TrainerController

`GET /api/trainer (getAll):` Retorna a lista de todos os treinadores cadastrados no banco.

`POST /api/trainer (create):` Realiza o cadastro de um novo treinador. Valida a obrigatoriedade dos campos, a validade do formato do email e a existência do email no banco.

### Registration Controller

`GET /api/registration (getAll):` Retorna a lista de todas as matrículas cadastradas no banco. Se algum filtro foi passado (busca por nome do treinador ou do Pokemon, ou por status), retorna a busca correspondente.

`POST /api/registration (create):` Realiza o cadastro de uma nova matrícula. Valida a obrigatoriedade dos campos, e as regras R1 e R3 (o Pokemon não pode ter matrícula ativa, e para estar na Elite dos 4, o nível deve ser >= 50).

`PUT /api/registration/{id}/cancel (cancel):` Cancela a matrícula do Pokemon. Verifica se a matrícula existe e se está ativa.

`POST /api/registration (upgrade):` Faz o upgrade da matrícula ativa do Pokemon. Verifica se há matricula ativa, checa o nível do Pokémon (R3) e faz o cálculo proporcional do upgrade. A matrícula é concluida, e é criada uma nova matrícula ativa. O valor proporcional é atualizado na matrícula ativa.

## Frontend

A aplicação foi dividida em duas abas: 
1. Área de Cadastros (Treinador/Pokémon)
  - Cadastro de Treinador
  - Cadastro de Pokémon
2. Matrículas & Upgrades
 - Nova Matrícula
 - Matrículas
 - Transferência de Treinador

Vamos aprofundar mais sobre cada um

Cadastro de Treinador: recebe o nome do treinador, email (que deve ser único) e cidade de origem do treinador. Mensagens amigáveis são exibidas abaixo em caso de sucesso ou fracasso.

Cadastro de Pokémon: recebe o nome do Pokémon, o tipo, o nível (entre 1 e 100) e o trainerId (no front, é um combobox populado com os treinadores cadastrados no banco). Verifica o preenchimento dos campos.

Nova Matrícula: recebe o Pokemon e o plano. Ambos são comboboxes (o Pokemon, populado pelo banco, e o plano é populado pelo html). 
- Além disso, para fins de teste do cálculo da mensalidade, foi inserido um campo `data de início`. Em uma aplicação real, essa data deve ser gerada pelo próprio código, que insere a data atual. Porém, como precisamos verificar a funcionalidade, é possível selecionar manualmente a data de início da matrícula
- Internamente, temos a função loadPokemons, que utiliza a rota `GET /api/pokemon (getAll)` para em seguida, popular o combobox

Matrículas Ativas: dashboard que exibe todas as matrículas. Nele, há as opções de upgrade de plano e de cancelar matrícula.
- Upgrade de plano: optei por fazer um upgrade por vez, ou seja, se o Pokemon está no plano Ginásio Local, ao clicar no Subir Plano, ele vai para o Liga Regional. Se clicar novamente (e obedecer à R3), ele vai para o Elite dos 4. Essa opção só aparece para matrículas ativas e nos planos Ginásio Local e Liga Regional. A matrícula antiga é concluida e a nova é ativa. Na matrícula concluida, é exibido o valor cheio da mensalidade, e na ativa, é exibido o cálculo porporcional.
- Cancelar plano: ao clicar, a matrícula é cancelada. O valor proporcional é calculado pelos dias de uso do plano.

Transferência de Treinador: é possível mudar o treinador de um pokemon. Basta escolher o Pokémon e o novo treinador, ambos comboboxes. Assim, o dashboard agora fica com o Treinador atualizado.

## Dificuldades encontradas

Não tinha familiaridade em programar em C#, apenas alguns exercícios na faculdade, então por si só, já foi um desafio. Porém, minha base em C++ e Java foi muito útil, facilitando o desenvolvimento. Além disso, o Angular era um framework que eu conhecia, mas tinha utilizado poucas vezes. Foi muito interessante utilizá-lo em um projeto completo.

Em relação à implementação, a maior dificuldade foi compreender e aplicar a interface reativa. Por exemplo, queria que, ao inserir um treinador, já aparecesse no Combobox de criação de pokemons o novo treinador, sem precisar recarregar a página. Para isso, utilizei os seguintes recursos:

1. @Output() e EventEmitter

    Usei o EventEmitter nos componentes de formulário, para fazer a comunicação do componente filho com o componente pai. Quando uma ação é concluida com sucesso (como a transferência de um Pokémon ou o cadastro de um treinador), o componente filho dispara um evento para que a tela principal saiba que precisa atualizar suas listagens e dashboards automaticamente, sem recarregar a página.

2. ChangeDetectorRef

    O Angular atualiza a tela automaticamente na maioria dos casos, mas em chamadas assíncronas (como requisições HTTP da API), pode haver um pequeno delay entre a chegada dos dados no JavaScript e a renderização no HTML — como no caso dos comboboxes que não atualizavam imediatamente. Usei o ChangeDetectorRef para forçar a sincronização instantânea entre os dados e a interface visual.

3. ngSubmit em formulários

    Substituí a lógica tradicional de clique em botões pelo (ngSubmit) nas tags `<form>`. O objetivo foi interceptar o envio nativo do formulário do HTML, evitando que a página recarregue ao submeter e permitindo aplicar validações customizadas no TypeScript antes de disparar as chamadas HTTP.

## Melhorias Futuras

1- Melhorar o visual estético
2- Implementar testes automatizados
3- Numa aplicação real, a data de início da matrícula seria a data em que a matrícula é criada
3- Autenticação e autorização, para controles de acesso

## IA durante o desenvolvimento

Durante o desenvolvimento, utilizei o Gemini PRO. Foi utilizado para correção de bugs, implementação da estrutura visual do dashboard (registration-dashboard.html) e entendimento dos recursos citados para fazer a interface reativa.

## Como executar o código

### Banco de dados

Requisitos: possuir docker instalado.

No terminal, rode
```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForte123!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

Configure no seu gerenciador de banco favorito (eu uso o DBGate) as credenciais.
```
Connection Type: Microsoft SQL Server.

Server: localhost

Port: 1433

User: sa

Password: SuaSenhaForte123!
```

Rode a query SQL encontrada na pasta database (db.sql) dentro do banco criado para criação das tabelas. Obs: o appsettings.json está configurado para essas variáveis acima. Caso deseje mudá-las, é necessário alterar esse arquivo também.

### Backend
Abra o terminal na raiz do projeto. Insira os comandos
```
cd backend/CentroPokemon.Api
```
```
dotnet run
```

### Frontend
Agora que o backend está rodando, vamos ao front. Abra um novo terminal na raiz do projeto e execute

```
cd frontend
```
```
ng serve
```

Basta entrar em http://localhost:4200 e testar!