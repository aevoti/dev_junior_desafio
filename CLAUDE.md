# CLAUDE.md

Contexto do projeto: veja DESAFIO.md (enunciado original) e .specify/ (specs formais geradas via Spec Kit).

## Estrutura
/backend, /frontend, /database

Antes de investigar a árvore de arquivos do zero (para corrigir um bug ou
entender a arquitetura), veja primeiro `specs/001-enrollment-management/plan.md`
(seção "Project Structure") e `data-model.md` — são os mapas do projeto
mantidos atualizados a cada rodada de mudanças, e evitam uma varredura
completa do repositório.

## Fluxo de trabalho (SDD)

- Antes de escrever qualquer código para corrigir um bug ou adicionar uma
  funcionalidade, atualize primeiro os artefatos do Spec Kit relevantes
  (spec.md, plan.md, tasks.md, contracts/api.md) quando a mudança afetar
  comportamento ou regra de negócio. Pare e deixe o usuário revisar o diff
  da documentação antes de implementar — só prossiga para o código depois
  de um sinal explícito de aprovação.
- Se uma correção puder ser implementada de mais de uma forma e essas
  formas mudarem comportamento real (não só a qualidade do código), pergunte
  ao usuário explicitando a consequência concreta de cada opção (ex.: "hoje
  acontece X, depois dessa mudança aconteceria Y") antes de escolher uma
  sozinho. A resposta pode ser um meio-termo que nenhuma das opções
  apresentadas cobria — esteja pronto para isso.
- Ao final de uma tarefa com mudanças de UI/comportamento, valide rodando a
  aplicação de ponta a ponta (não só testes automatizados/build) antes de
  considerar a tarefa concluída.

## Commits

- Mensagens de commit curtas, de uma linha, no padrão já usado no
  histórico do repositório (`tipo: descrição curta`, ex.: `fix: ...`,
  `feat: ...`, `docs: ...`) — sem corpo com bullets.
- Preferir vários commits pequenos agrupados por unidade lógica em vez de
  um único commit com tudo.
- Nunca dar `git add`/`git commit` em `README.md` a menos que o usuário peça
  explicitamente — esse arquivo é commitado por último, pelo próprio
  usuário, mesmo que o resto do trabalho já tenha sido commitado em partes.

## Idioma — regra obrigatória para todo conteúdo gerado neste projeto

| Tipo de conteúdo | Idioma |
|---|---|
| Nomes de variáveis, classes, métodos, propriedades, tabelas de banco | Inglês |
| Mensagens de erro internas (exceptions, logs) | Inglês |
| Documentação de método (XML doc /// summary, JSDoc) | Inglês |
| Nomes de commit, branch, arquivo, projeto | Inglês |
| Comentários soltos no código (explicando lógica de negócio) | Português |
| Arquivos do Spec Kit: constitution.md, spec.md, plan.md, tasks.md | Português |
| Mensagens de erro exibidas ao usuário final (API response, frontend) | Português |
| README.md | Português |

Antes de gerar qualquer arquivo, verifique qual categoria ele se encaixa nesta tabela.
Em caso de dúvida sobre uma categoria não listada aqui, pergunte antes de assumir.