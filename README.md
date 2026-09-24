# Shooter Toy - Desafio 3

Run-and-gun lateral no estilo Metal Slug, numa floresta noturna. Voce e o Tarma, os
rebeldes chegam em hordas, e cada um deles morre com um tiro so - assim como voce.

No Desafio 2 isto era um brinquedo: inimigos para sempre, sem objetivo. No Desafio 3
virou um jogo, construido em volta de uma experiencia:

> Quero que o jogador sinta **ganancia** - que cada moeda gasta para salvar a pele agora
> seja uma arma que ele nao vai ter na proxima horda.

Projeto Unity **6000.5.10f1**, 2D com URP, Input System novo e Cinemachine.
Abrir `Assets/Scenes/SampleScene.unity` e dar Play, ou jogar a versao WebGL (link na
entrega da atividade).

## Como jogar

| Tecla | Acao |
|---|---|
| `A` `D` / setas | anda |
| `Espaco` | pula |
| `J` ou botao esquerdo do mouse | atira (um tiro por toque; a metralhadora repete segurando) |
| `K` | **granada**: mata todos os rebeldes dentro do cerco, custa moedas |
| `E` (perto do prisioneiro) | abre a loja; `1`-`5` compram, `E` ou `Esc` fecham |
| `P` ou `Esc` | pausa (no navegador o `Esc` e do browser, entao use o `P`) |
| `T` (no menu) | modo de teste: comeca na horda 4, com shotgun, 2 coletes e 30 moedas |

## O loop do jogo

1. **Horda.** Duas paredes fecham um **cerco** em volta do jogador. A horda so acaba
   quando todos os rebeldes dela morrem (8, 12, 16 e 20 nas hordas 1 a 4).
2. **Moedas no chao.** Cada rebelde tem 75% de chance de soltar moeda (1 da faca, 2 do
   fuzil). A moeda **some em 7 segundos**: quem fica parado atirando fica pobre, e e
   ela que puxa o jogador para o meio do tiroteio. Matar em sequencia paga bonus, e
   limpar a horda paga +5.
3. **Loja do prisioneiro.** Quando o cerco abre, um prisioneiro aparece no caminho.
   Chegando perto ele se solta e vende:

   | Item | Preco | Efeito |
   |---|---|---|
   | Colete | 12 | absorve um golpe (ate 3) |
   | Shotgun | 22 | 3 chumbos em leque, alcance curto |
   | Metralhadora | 30 | atira segurando o botao |
   | Rocket | 45 | explode em area |
   | Segunda chance | 60 | ao morrer, renasce e o cerco explode (uma por partida) |

   Nenhuma arma mata mais que outra: todo inimigo morre com um acerto. O que muda e a
   **forma** do tiro. Uma arma por vez; acabou a municao, volta a pistola.
4. **Andar para frente** dispara a horda seguinte.
5. **Horda 5, a ultima.** A regra troca: em vez de abates, o jogador precisa
   **sobreviver** 15 segundos num cerco mais apertado, com rebeldes sem parar. Depois
   entra o **chefe**, o rebelde da Minigun, com barra de vida. Derrubar ele vence o jogo.

A tela de vitoria mostra quantas moedas sobraram: o recorde que importa e vencer rico.

## Como o jogo cumpre o Desafio 3

| Pontos | Exigencia | O que o jogo tem |
|---|---|---|
| 50 | Menu de entrada, pause e game over | Menu com controles e recorde, pausa, fim de jogo comparando com o recorde e vitoria. Uma cena so, com uma maquina de estados (`Estado` no `GameManager`) |
| 50 | Camera especial **ou** IA de inimigo | **Cinemachine**: look-ahead na direcao do movimento, camera presa nas paredes do cerco (desliza ao prender e ao soltar) e tremor na granada |
| 50 | Acao especial com vantagem e punicao | **Granada** (K): limpa o cerco, mas cobra na hora do uso (15, 20, 25...), nao solta moeda dos mortos e so pode ser usada **uma vez por horda** |
| 100 | Sistema de recompensas | Moedas que expiram no chao, bonus de sequencia e de horda, loja com colete, armas e segunda chance |
| 150 | Criatividade na experiencia | O loop inteiro gira em torno de gastar ou guardar, com o chefe como prova final do dinheiro guardado |

## Como o projeto esta organizado

```
Assets/
  Art/                fundo NightForest e quadros de animacao (1 PNG por quadro)
  Animations/         clipes e Animator Controllers do Tarma e dos rebeldes
  Fonts/              Comic Sans (ComicSansMS.ttf) dentro do projeto, para o WebGL
  Prefabs/            rebeldes, balas
  Resources/          prisioneiro e chefes, carregados por nome em codigo
  Scenes/             SampleScene - a unica cena do jogo
  Scripts/            scripts (abaixo)
Fontes/               folhas de sprite originais + creditos da arte
```

### Scripts

| Script | O que faz |
|---|---|
| `GameManager` | maquina de estados (menu, jogando, pausa, loja, fim, vitoria), placar, moedas e recordes |
| `HUD` | desenha todas as telas e o placar em `OnGUI` |
| `Horda` | comeca e termina as hordas, paga os bonus, chama o chefe e o prisioneiro |
| `Cerco` | as duas paredes que prendem o jogador durante a horda |
| `CameraNoCerco` | extensao do Cinemachine que prende a camera nas paredes |
| `Spawner` | cria os rebeldes de cada horda, mais rapido a cada horda |
| `PlayerScript` | correr, pular, atirar com cada arma, colete, morte |
| `Arsenal` | arma atual, municao, coletes e segunda chance |
| `Granada` | a acao especial: explosao, preco crescente, limite por horda, tremor |
| `Moeda` | a moeda no chao, com prazo para ser pega |
| `Loja` / `Prisioneiro` | a loja entre as hordas e quem atende |
| `Chefe` / `Obus` | o chefe com barra de vida e o tiro em arco |
| `EnemyKnife` / `EnemyRifle` / `EnemyDeath` | os rebeldes e a morte que os dois compartilham |
| `BulletScript` / `Explosao` | as balas (inclusive o rocket) e o clarao da explosao |
| `Sangue` / `Quadros` | efeitos de abate e animacao simples por lista de quadros |
| `ParallaxLayer` / `GroundScroller` | fundo infinito em parallax e o chao que acompanha a camera |
| `CameraFollow` | camera do Desafio 2, desligada desde a entrada do Cinemachine |

### Decisoes que valem explicacao

**Uma cena so, uma maquina de estados.** O menu, a pausa, a loja e o fim de jogo nao
sao outras cenas: sao estados do `GameManager`, e so o `Jogando` deixa o tempo andar.
Todo o `Time.timeScale` passa por um metodo so (`IrPara`), o que evita a tela que
congela o jogo e esquece de descongelar.

**O mundo e infinito sem fabricar cenario.** Cada camada do fundo e um sprite em modo
*Tiled* que o `ParallaxLayer` reposiciona por repeticoes inteiras, e o chao e um unico
colisor que acompanha a camera. O cerco e o que da tamanho ao mundo durante a horda.

**A fonte vem dentro do projeto.** No Desafio 2 a Comic Sans era carregada das fontes
do Windows; no navegador ela nao existe e o texto sumia. Agora a fonte e um asset.

## Creditos da arte

Sprites de personagem de **Metal Slug 3**, propriedade da **SNK/Playmore**, ripados por
**Gussprint** (o pacote exige credito) e **Goemar** (folha da Minigun):

> COPYRIGHTED BY: SNK/Playmore - TILE-RIPPED BY: Gussprint - REQUIREMENTS FOR USE: Give credit.

Cenario **NightForest**, de pacote separado. As folhas originais e a atribuicao
completa estao em [`Fontes/CREDITOS.md`](Fontes/CREDITOS.md).

Uso academico, sem fins comerciais.
