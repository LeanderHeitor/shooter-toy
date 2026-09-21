# Handoff - Shooter Toy / DIU3

Sessão de 18/09/2026 (sexta). Projeto Unity 2D em
`C:\Users\heitor.silva\Desktop\shooter-toy`, branch `main`, limpa no início da sessão.

## Onde ler antes de qualquer coisa

Não repito aqui o que já está nos arquivos do repo:

| Arquivo | O que tem |
|---|---|
| `DIU3 - Survival Game - Next Generation.md` | O enunciado. 400 pontos, itens obrigatórios |
| `Dicas para cumprir DIU3.md` | Roteiro de 7 dias sugerido pelo professor |
| `CONTEXT.md` | **Glossário.** Foi reescrito nesta sessão. Leia inteiro antes de codar |
| `PLANO_DIU3.md` | **Plano.** Economia, hordas, calendário, ordem de corte. Criado nesta sessão |
| `README.md` | Estado do DIU2. Ainda não menciona nada do DIU3 |
| `PLANO_DESAFIO2.md` | Plano do desafio anterior, histórico |

## Prazo

**Quinta-feira, 24/09/2026.** Calendário dia a dia está no `PLANO_DIU3.md`.
O domingo 20/09 foi deixado livre de propósito, como reserva.

## O que foi feito nesta sessão

1. Sessão de grilling completa (4 rodadas). Todas as decisões de design fechadas.
2. `CONTEXT.md` reescrito: 9 termos novos, 4 definições antigas corrigidas.
3. `PLANO_DIU3.md` criado.
4. Primeiro item de código: a fonte (detalhes abaixo).

**Nada foi commitado ainda.** `git status` vai mostrar 4 arquivos modificados/novos.

## Estado do código - IMPORTANTE, NÃO VERIFICADO

O trabalho de maior risco do plano (Comic Sans no WebGL) foi adiantado, mas
**não foi compilado nem testado**:

- `Assets/Fonts/ComicSansMS.ttf` - cópia de `C:\Windows\Fonts\comic.ttf`, já importada
  pelo Unity (guid `9ccc7bad66d211f41a596e95973eb22e`)
- `Assets/Scripts/HUD.cs` - trocado `Font.CreateDynamicFontFromOSFont` por um campo
  `public Font fonte`. O `OnDestroy` que dava `Destroy(fonte)` foi removido de
  propósito: com asset ele apagaria o arquivo do projeto
- `Assets/Scenes/SampleScene.unity` - a referência da fonte foi escrita **direto no
  YAML** do componente HUD (que fica no GameObject `GameManager`)

**Primeira coisa a fazer na próxima sessão:** pedir para o usuário clicar na janela do
Unity (força o recompile), depois dar Play e confirmar que os textos aparecem em Comic
Sans. Se o Unity perguntar sobre a cena, é **recarregar do disco**, não salvar por cima.

Também foi encontrado e removido um campo órfão no YAML do HUD (`tamanhoDaLetra: 22`,
de uma versão antiga do script). Sem efeito visual.

## Unity MCP - como ele se comporta aqui

O MCP oficial da Unity (`com.unity.ai.assistant`) foi instalado pelo usuário durante a
sessão e está conectado. Dois servidores aparecem na lista (`mcp__unity__` e
`mcp__unity-mcp__`); usei o `mcp__unity__` e funcionou.

**Armadilha que custou tempo:** o Editor **só recompila scripts quando a janela ganha
foco**. Enquanto ele está sem foco, `Unity_ManageGameObject` com
`set_component_property` falha com "Property not found" para qualquer campo novo, e
nem `Assets/Refresh` via `Unity_ManageMenuItem` resolve. Quando isso acontecer:
peça o foco ao usuário, ou escreva no YAML da cena como fiz.

`Unity_ReadConsole` e `Unity_ManageAsset` (Import) funcionam sem foco.

Warnings benignos no console que podem ser ignorados: coleta de assinatura de um
`claude.exe.old.*` órfão, e "Account API did not become accessible" quando o Editor
está sem foco.

## Decisões já fechadas - NÃO reabrir

O racional completo está no `PLANO_DIU3.md`. O que importa aqui é que **já foram
discutidas e decididas**, então não vale a pena propor alternativas de novo:

- Mundo: **cerco elástico** (não é arena com Tilemap, e não é o corredor infinito puro)
- Câmera: **Cinemachine** (não IA de inimigo). O pacote **ainda não foi instalado**
- Defesa: **colete que absorve 1 golpe**, nunca barra de vida. O "morre com um tiro"
  é a identidade do jogo e está protegido no `CONTEXT.md`
- Armas: diferem na **forma do tiro**, nunca em dano. Uma arma por vez
- Menus: **máquina de estados numa cena só**, desenhada em `OnGUI`. Sem Canvas,
  sem cenas separadas
- Morte reseta **tudo**: horda 1, sem moedas, sem coletes, só pistola
- Prisioneiro: **quadrado amarelo** por enquanto. A folha do POW virá depois
- Fonte: o usuário escolheu o `comic.ttf` da Microsoft, ciente de que existe a
  alternativa livre (Comic Neue). Foi decisão dele; não reabrir

### Dois pontos onde o usuário decidiu contra a minha recomendação

Registro para o próximo agente não gastar tempo re-argumentando:

1. **75% de chance de drop de moeda.** Eu argumentei que aleatoriedade atrapalha a
   experiência de ganância (o jogador não consegue planejar). O usuário pediu duas
   vezes. Está implementado como campo ajustável justamente para subir/descer em
   playtest.
2. **Escopo completo, sem cortes prévios.** Eu propus cortar Rocket, colete
   empilhável e multiplicador de sequência por causa do prazo. O usuário quis fazer
   tudo e cortar depois se precisar. A resposta foi **ordenar** o trabalho: a lista de
   corte no fim do `PLANO_DIU3.md` só contém folhas, que podem ser abandonadas na
   quarta sem desfazer nada.

## Próximos passos, em ordem

1. Recompilar (foco na janela) e validar a fonte no Play
2. **Build WebGL de teste** com o jogo como está, subir no play.unity.com.
   É o item que decide se a semana é tranquila ou corrida
3. Segunda: máquina de estados + 3 menus + Cinemachine (100 pts)
4. Terça: cerco elástico + hordas + moedas (o maior dia)
5. Quarta: loja, prisioneiro, armas, granada, Horda de Resistência
6. Quinta: build final, README, vídeo de até 10 min, GitHub. Sem código novo

O `README.md` ainda descreve só o DIU2 e vai precisar ser reescrito na quinta.

## Sobre trabalhar com este usuário

- Fala e escreve em **português**. Todo o repo (código, comentários, docs) é PT-BR
- É estudante; pede explicação quando não conhece um conceito ("o que seria máquina de
  estados?", "como assim navegador?"). Explique com exemplo concreto, sem jargão
- Gosta de propor ideias novas no meio da execução. Várias foram boas (munição na
  loja, horda de resistência a cada 5). Vale avaliar de verdade em vez de aceitar ou
  recusar de imediato - mas também vale lembrar do relógio
- Os comentários do código existente são didáticos e explicam o *porquê*, não o *quê*.
  Mantenha esse estilo (veja `PlayerScript.cs` e `Spawner.cs` como referência)

## Suggested skills

Chame com a ferramenta Skill:

- **`mattpocock-skills:domain-modeling`** - sempre que um termo novo aparecer ou uma
  definição do `CONTEXT.md` for contrariada pelo código. O glossário é a espinha do
  projeto e foi construído com cuidado; mantenha-o atualizado na hora, não em lote
- **`mattpocock-skills:grilling`** - se o usuário propuser mudança grande de escopo.
  Ele responde bem a perguntas numeradas com recomendação
- **`mattpocock-skills:diagnosing-bugs`** - quando o build WebGL quebrar (é provável
  que quebre em alguma coisa além da fonte)
- **`claude-in-chrome`** - para validar o build publicado no play.unity.com no fim
