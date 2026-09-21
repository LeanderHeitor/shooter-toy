# Handoff - Shooter Toy / DIU3

Atualizado na segunda, 21/09/2026. Projeto Unity 2D em
`C:\Users\heitor.silva\Desktop\shooter-toy`, branch `main`, igual ao `origin/main`
(GitHub `LeanderHeitor/shooter-toy`).

## Onde ler antes de qualquer coisa

| Arquivo | O que tem |
|---|---|
| `DIU3 - Survival Game - Next Generation.md` | O enunciado. 400 pontos, itens obrigatórios |
| `Dicas para cumprir DIU3.md` | Roteiro de 7 dias sugerido pelo professor |
| `CONTEXT.md` | **Glossário.** Leia inteiro antes de codar. Está no `.git/info/exclude`: não vai para o GitHub |
| `PLANO_DIU3.md` | **Plano.** Economia, hordas, calendário, ordem de corte |
| `README.md` | **Desatualizado:** ainda descreve só o DIU2 |
| `ROTEIRO_VIDEO.md` | **Desatualizado:** roteiro do Desafio 2. Também está no exclude |

## Prazo

**Quinta-feira, 24/09/2026.** Todo o código do plano está pronto. O que sobra é entrega.

## Estado: todo o escopo do plano está implementado

Nada da lista de corte do `PLANO_DIU3.md` precisou ser cortado.

| Commit | O que entrou |
|---|---|
| `672a7c9` | Fonte Comic Sans como asset (WebGL), máquina de estados, menus em `OnGUI` |
| `e5c629d` | Moeda como objeto no chão, com prazo de 7 s |
| `a0304bd` | Cerco elástico, hordas de quota, Horda de Resistência, Cinemachine |
| `5dd9cc3` | Granada (K, 10 moedas, tremor pelo Impulse) |
| `89d1d2a` | Loja, prisioneiro, colete, shotgun, metralhadora, rocket |

### Como a loja ficou (último commit)

- `Horda.Terminar` chama o prisioneiro na **metade do caminho** até onde a próxima horda
  começa (`avancoParaComecar * 0.5`). `Horda.Comecar` o dispensa.
- O prisioneiro nasce amarrado (`POW_Tied`) e se solta (`POW_Idle`) quando o jogador
  chega perto. Não usa colisor, só a distância em x.
- **E** (ou W / seta para cima) abre a loja; **E ou Esc** fecha; **1-4** compram. O
  estado novo `Estado.Loja` congela o mundo como a pausa.
- `GameManager.quadroDaUltimaTroca` impede que uma tecla troque de tela duas vezes no
  mesmo quadro (o Esc que fecha a loja virava pausa, por causa da ordem dos `Update`).
- A `Loja` é criada por `AddComponent` no `GameManager.Awake`. **A cena não foi
  editada.** O prisioneiro é um prefab em `Assets/Resources/Prisioneiro.prefab`.
- `Arsenal` é uma classe static (arma, munição, coletes), zerada no
  `GameManager.Awake`.
- Colete: absorve o golpe e dá `invencivelDepoisDoColete` (1,2 s) piscando.
- Rocket: `BulletScript.raioDaExplosao > 0`, mata pela distância em x e desenha o
  círculo do `Explosao.cs`.

### O que foi testado e o que não foi

Testado no Play **por comandos do MCP**, sem teclado:
- posição do prisioneiro
- preços e cobrança
- recarga de munição e aviso de troca de arma
- 3 balas da shotgun
- volta à pistola no último tiro
- colete absorvendo o golpe
- rocket matando exatamente os inimigos dentro do raio
- reset de tudo na morte
- screenshot da tela da loja

**Nunca testado com teclado de verdade:**
- entrar e sair da loja com E e Esc
- segurar J com a metralhadora
- o balanço da economia

Ponto a observar: com um rebelde da faca grudado, 3 coletes somem em ~4 s (um a cada
1,2 s). Se incomodar, a correção é empurrar o rebelde para trás quando o colete quebra.

## O que falta, em ordem

1. **Playtest com teclado** (os itens acima). Os 75% de drop continuam sendo o primeiro
   número a mexer se a economia ficar sovina
2. **Build WebGL novo.** `Build/WebGL` e `shooter-toy-webgl.zip` são de 19/09, de
   antes de tudo do DIU3. Gerar de novo, subir no play.unity.com, testar no navegador.
   No navegador o Esc é do browser: o P existe como pausa garantida
3. **README** reescrito para o DIU3
4. **Roteiro e vídeo** de até 10 min
5. Push final no GitHub

### Fora do plano, pendente de decisão do usuário

A arte de chefes para as hordas 5 e 10 entrou no commit `e0e1212`, em
`Assets/Art/Characters/BOSS HORDER 5/` e `BOSS HORDER 10/`. **Não existe código nem
plano para chefe**, e hoje a horda 5 é a de Resistência. Recomendei deixar para depois
da entrega; o usuário ainda não respondeu.

O usuário depois disse que não precisava ter subido a arte. Ofereci removê-la (commit
apagando, ou force push tirando do histórico) e **ele ainda não escolheu**. Não mexer
sem ele pedir.

`Assets/_Recovery/` é resto de crash do Unity e ficou fora do git de propósito.

## Unity MCP - como ele se comporta aqui

Use o servidor `mcp__unity__` (aparecem dois; este funciona).

- `Unity_RunCommand` **bloqueia `System.Reflection`**. Para chamar método privado, use
  `SendMessage("NomeDoMetodo", arg)`
- Armadilha: `SendMessage("X", 0)` com o **literal** 0 cai na sobrecarga de
  `SendMessageOptions` e o método não recebe o argumento. Passe uma variável `int`
- `GetInstanceID()` está obsoleto nesta versão; é `GetEntityId()`. O ID é grande demais
  para o JSON do `Unity_Camera_Capture`, que falha. Para ver a tela, use
  `ScreenCapture.CaptureScreenshot("<scratchpad>/x.png")` no Play e leia o PNG
  (pega o `OnGUI` também)
- `Assets/Refresh` pelo `Unity_ManageMenuItem` compilou e importou sem precisar do
  foco na janela nesta sessão. Na anterior não funcionava: se um campo novo não
  aparecer, peça ao usuário para clicar na janela do Unity
- Em teste, lembre que a horda começa sozinha 12 s depois do cerco abrir
  (`segundosAteComecarSozinha`). Faça o que depende do intervalo num comando só
- Warnings benignos: "Account API did not become accessible" e a assinatura de um
  `claude.exe.old.*` órfão

## Decisões já fechadas - NÃO reabrir

- Mundo: **cerco elástico**
- Câmera: **Cinemachine** (não IA de inimigo)
- Defesa: **colete que absorve 1 golpe**, nunca barra de vida
- Armas: diferem na **forma do tiro**, nunca em dano. Uma arma por vez
- Menus: **máquina de estados numa cena só**, em `OnGUI`. Sem Canvas
- Morte reseta **tudo**
- Fonte: `comic.ttf` da Microsoft, escolha do usuário
- **75% de drop** e **escopo completo** foram decididos pelo usuário contra a minha
  recomendação. Não re-argumentar

## Sobre trabalhar com este usuário

- Fala e escreve em **português**. Todo o repo é PT-BR
- **Commits:** em português, curtos, sem travessão e **sem nenhuma menção ao Claude**
  (nada de Co-Authored-By nem link de sessão). É trabalho acadêmico. Ele reforçou isso
  nesta sessão
- Commit é local: ele espera que "subir" signifique **push**. Confirme o push quando
  disser que algo subiu
- É estudante; explique conceitos com exemplo concreto, sem jargão
- Propõe ideias novas no meio da execução. Avalie de verdade, mas lembre do relógio
- Comentários do código são didáticos e explicam o *porquê*. Mantenha o estilo
- Os arquivos misturam CRLF e LF. Preserve o que cada arquivo já usa

## Suggested skills

- **`mattpocock-skills:domain-modeling`**: ao aparecer termo novo (ex.: "chefe") ou o
  código contrariar o `CONTEXT.md`
- **`mattpocock-skills:diagnosing-bugs`**: quando o build WebGL quebrar
- **`claude-in-chrome`**: para validar o build publicado no play.unity.com
