# Shooter Toy - Desafio 2

Run-and-gun lateral de sobrevivencia sem fim, ambientado numa floresta noturna.
Voce e o Tarma (Metal Slug), os rebeldes nao param de chegar e cada um deles morre
com um tiro so - assim como voce.

Projeto Unity **6000.5.10f1**, 2D com URP e o Input System novo.

## Como jogar

| Tecla | Acao |
|---|---|
| `A` / seta esquerda | anda pra esquerda |
| `D` / seta direita | anda pra direita |
| `Espaco` | pula (so no chao) |
| `J` ou botao esquerdo do mouse | **um tiro por toque**, ate 4 tiros/s; municao infinita |

Abrir `Assets/Scenes/SampleScene.unity` e dar Play.

Objetivo: sobreviver e superar seu numero de **abates**. Segurar o tiro nao cria
novas balas; toques feitos durante o intervalo de 0,25s sao ignorados.

Quando voce morre, a animacao termina e aparece **VOCE MORREU**, com o jogo congelado.
Uma nova tecla ou clique reinicia a partida. O **recorde** da sessao sobrevive ao
reinicio. A **sequencia** aparece a partir de dois abates e expira apos 3s sem abater.
Todos os textos usam **Comic Sans MS**, carregada das fontes instaladas no sistema.

## Regras

- Um toque do rebelde da faca, ou um tiro do rebelde do fuzil, mata voce.
- Um tiro seu mata qualquer inimigo.
- Ao renascer voce fica **1,5s invencivel, piscando**, pra nao morrer no susto.
- Os inimigos nascem pra sempre e cada vez mais rapido:

| | |
|---|---|
| Intervalo entre inimigos | 2,5s no inicio, chegando a 0,6s aos 2min30 |
| Maximo vivos ao mesmo tempo | 6 |
| So rebelde da faca | antes de 10s |
| Primeiro rebelde do fuzil | primeiro spawn com vaga a partir de 10s |
| Mistura depois do primeiro fuzileiro | chance de fuzil de 25%, crescendo ate 50% aos 2min30 |
| Inimigo pelas costas | so depois de 60s |

O fuzileiro para a 6 unidades e dispara sem espera adicional antes do primeiro tiro;
os seguintes respeitam um intervalo de 2s. Sua bala e maior e mais lenta (velocidade 6)
que a do jogador (16). Esses valores sao pontos de partida para o balanceamento.
Inimigos mortos liberam a vaga de spawn imediatamente, antes de sumirem da tela.

## Como o projeto esta organizado

```
Assets/
  Art/Background/     6 camadas do NightForest usadas no parallax
  Art/Characters/     quadros de animacao, 1 PNG por quadro
  Animations/         9 clipes .anim + 3 Animator Controllers
  Prefabs/            KnifeRebel, RifleRebel, Bullet, EnemyBullet
  Scenes/             SampleScene - a unica cena do jogo
  Scripts/            11 scripts (abaixo)
Fontes/               folhas de sprite originais + creditos da arte
```

### Scripts

| Script | O que faz |
|---|---|
| `PlayerScript` | correr, pular, atirar por toque, morrer e piscar no respawn |
| `BulletScript` | a bala anda, mata quem acerta e some no fim do alcance |
| `EnemyDeath` | a morte de 1 tiro que os **dois** inimigos usam |
| `EnemyKnife` | corre atras do player e mata no encostao |
| `EnemyRifle` | anda, para na distancia de tiro e atira |
| `Spawner` | cria inimigos pra sempre, cada vez mais rapido |
| `GameManager` | abates, recorde, sequencia, fim de jogo e reinicio da cena |
| `HUD` | desenha os numeros e a tela de fim de jogo |
| `CameraFollow` | camera segue o player na horizontal |
| `ParallaxLayer` | rola uma camada do fundo e a repete pra nunca acabar |
| `GroundScroller` | mantem o colisor do chao sempre embaixo da camera |

### Duas decisoes que valem explicacao

**O mundo e infinito sem fabricar cenario.** Em vez de instanciar pedacos de chao,
cada camada do fundo e um sprite em modo *Tiled* que o `ParallaxLayer` reposiciona por
um numero inteiro de repeticoes a cada quadro - como a imagem se repete, o salto e
invisivel e a camada nunca acaba. O chao e um unico colisor que acompanha a camera.

**A morte do inimigo mora num componente separado.** `EnemyDeath` existe porque a bala
precisa de um jeito unico de matar qualquer inimigo, sem herdar classe nem cruzar tipo.
Os dois inimigos tambem compartilham o mesmo clipe de morte (`Rebel_Death`).

## Numeros da cena

- Pixels Per Unit **32** nos personagens e no fundo; camera ortografica **size 5.625**
  (o fundo de 360px preenche a tela exatamente).
- Linha do chao em **y = -3.75**. O pivo dos personagens e inferior-central,
  entao o y do transform deles no chao e exatamente esse valor.
- Os rebeldes usam uma celula unica de **52x51**, porque os dois compartilham a
  animacao de morte. O Tarma usa **52x44** na maioria dos clipes, **48x42** na
  corrida e **60x44** na morte; como o pivo e o centro da base, isso nao o desalinha.
- O fundo tem 9 objetos montados com as 6 camadas. Os que rolam vao do fator
  **0.15** (ceu) ao **1.0** (mato); as camadas **3 e 5** (raios de luz e nevoa)
  tem fator 0 e ficam presas na camera.

## Entrega do Desafio Unity 2

O jogo preserva as tres acoes (correr, pular, atirar), animacoes do personagem e dos
inimigos, morte e reinicio, morte e respawn dos inimigos e dificuldade gradual.
O estudo prioriza codigo C# curto e legivel, com um script de bala compartilhado
pelos dois prefabs e morte compartilhada pelos dois tipos de inimigo.

Grave um video de **ate 10 minutos** explicando o jogo, o que aprendeu e uma
dificuldade superada. Disponibilize o projeto com scripts e assets no GitHub e
poste o link junto da entrega.

## Creditos da arte

Sprites de personagem de **Metal Slug 3**, propriedade da **SNK/Playmore**,
ripados por **Gussprint** - o proprio pacote exige credito:

> COPYRIGHTED BY: SNK/Playmore - TILE-RIPPED BY: Gussprint - REQUIREMENTS FOR USE: Give credit.

Cenario **NightForest**, de pacote separado. As folhas originais e a atribuicao
completa estao em [`Fontes/CREDITOS.md`](Fontes/CREDITOS.md).

Uso academico, sem fins comerciais.
