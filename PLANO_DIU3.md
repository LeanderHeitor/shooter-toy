# Plano do DIU3 - Hordas, Moedas e Ganância

Continuação do DIU2. O glossário dos termos está no `CONTEXT.md`; aqui ficam os
números, a ordem do trabalho e as decisões de implementação.

**Entrega: quinta-feira, 24/09/2026.**

## A experiência

> Quero que o jogador sinta **ganância** - que cada moeda gasta para salvar a pele
> agora seja uma arma que ele não vai ter na próxima horda.

Todo o resto do plano existe para servir essa frase. Quando uma decisão estiver em
dúvida, ganha a opção que deixa a escolha "gastar agora ou guardar" mais visível.

## Como isso cobre os 400 pontos

| Pontos | Exigência do enunciado | O que entrega |
|---|---|---|
| 50 | Menu de entrada, pause e game over | Máquina de estados numa cena só, desenhada em `OnGUI` |
| 50 | Câmera especial **ou** IA de inimigo | Cinemachine: look-ahead, dead zone, confiner no cerco, shake na granada |
| 50 | Ação especial com vantagem clara **e** punição | Granada: limpa o cerco, cobra moedas no instante do uso |
| 100 | Sistema de recompensas divertido e desafiador | Hordas + moedas dropadas + loja do prisioneiro |
| 150 | Criatividade na experiência | O loop inteiro: cerco, ganância, Horda de Resistência |

## A economia

```
GANHO - a moeda cai no chão e some em 7 segundos; quem não pega, não ganha
  rebelde da faca ............... 1 moeda    75% de chance de dropar
  rebelde do fuzil .............. 2 moedas   75% de chance de dropar
  multiplicador de sequência .... +1 moeda por abate a cada 5 de sequência
  horda limpa ................... +5 moedas
  Horda de Resistência vencida .. +10 moedas (o bônus dobra; o drop por abate NÃO)

GASTO NA LOJA (prisioneiro, entre hordas)
  Colete ........................  8   absorve 1 golpe, empilha até 3
  Shotgun ....................... 15   3 chumbos em leque, 20 tiros
  Metralhadora .................. 20   automática (ignora o 1-toque-1-tiro), 60 tiros
  Rocket ........................ 30   explosão em área, 8 tiros
  Munição shotgun ...............  8   10 tiros
  Munição metralhadora .......... 10   30 tiros
  Munição rocket ................ 15    4 tiros
  (munição acabou -> volta para a pistola infinita)

GASTO EM COMBATE
  Granada ....................... 10   mata tudo dentro do cerco, tecla K
```

**A moeda é um objeto, não um crédito.** Ela cai onde o inimigo morreu e expira em 7
segundos, então o cerco deixa de ser só uma parede: vira o motivo para o jogador
*avançar* para dentro do tiroteio. A coleta é um `OnTriggerEnter2D`, exatamente como
as dicas do desafio sugerem no dia 2.

Os 75% são um campo ajustável, não uma verdade: se em playtest a economia ficar
sovina, esse é o primeiro número a subir. A munição custa **o mesmo por tiro** que a
arma nova - refil é conveniência para quem está pobre, nunca um atalho mais barato,
senão o jogador compra a shotgun uma vez e ignora as outras armas para sempre.

**Uma arma por vez.** Comprar outra substitui a atual e perde a munição que sobrou,
como no Metal Slug. A loja só mostra a linha de munição da arma que o jogador
carrega, então a primeira visita lista 4 itens, não 8.

A granada cobra no uso e não na compra **de propósito**: é o que põe a decisão de
ganância dentro do combate, onde ela dói. Se ela virar munição comprada na loja, o
item de 50 pontos do enunciado enfraquece.

## O ritmo das hordas

```
Horda 1..4    QUOTA         8, 12, 16, 20 inimigos          bônus  +5
Horda 5       RESISTÊNCIA   sobreviva 30s, spawn contínuo   bônus +10
Horda 6..9    QUOTA         24, 28, 32, 36                  bônus  +5
Horda 10      RESISTÊNCIA   sobreviva 30s, mais rápido      bônus +10
...
```

O HUD mostra "restam N" na horda de quota e um cronômetro regressivo na de
resistência. O cerco fica visualmente mais fechado na de resistência, para que o
jogador leia a troca de regra sem ninguém explicar.

## O cerco elástico

O mundo continua infinito (o `ParallaxLayer` e o `GroundScroller` do DIU2 ficam como
estão). Quando uma horda começa, nascem duas barreiras a ~12 unidades de cada lado do
jogador, com visual claro nas bordas. O Confiner do Cinemachine usa esses mesmos
limites, então a câmera para junto e o jogador **sente** a caixa.

Limpou a horda: as barreiras somem, o prisioneiro entra, o mundo volta a rolar.
Andar para frente dispara a horda seguinte, mais adiante.

## Estados do jogo

```
Menu -> Jogando -> Pausado -> Jogando
                -> Loja    -> Jogando
                -> FimDeJogo -> Menu
```

Um `enum Estado` no `GameManager` no lugar do `bool isFimDeJogo` atual, e um `switch`
no `HUD`. Uma cena só, sem Canvas, sem `SceneManager` para troca de tela. Pausa e fim
de jogo continuam usando `Time.timeScale = 0`, como já funciona hoje.

## Ordem do trabalho

Sequenciada por risco e por ponto, de forma que o que sobrar para o fim seja o mais
barato de perder. Nada aqui foi cortado: o escopo é completo.

### Sábado 19 - WebGL primeiro

O risco número um. `HUD.cs:31` usa `Font.CreateDynamicFontFromOSFont("Comic Sans MS")`,
que **não funciona no navegador** - a fonte do sistema não existe lá, e todo o texto
do jogo sumiria. Copiar `C:\Windows\Fonts\comic.ttf` para `Assets/Fonts/`, importar e
referenciar o asset. Depois: build WebGL do jogo **como ele está hoje** e subir no
play.unity.com. Se o DIU2 não roda no navegador, nada mais importa.

### Segunda 21 - Menus e câmera (100 pts)

Máquina de estados no `GameManager`, os três menus em `OnGUI`, e o Cinemachine
instalado com a Virtual Camera substituindo o `CameraFollow.cs`.

### Terça 22 - O núcleo (100 pts)

Cerco elástico, hordas com quota, moedas dropando e sendo contadas. É o maior dia.

### Quarta 23 - Loja, armas e granada (50 pts)

Prisioneiro (quadrado amarelo por enquanto), tela de loja, colete, shotgun,
metralhadora, granada na tecla K, e a Horda de Resistência.

### Quinta 24 - Entrega. Nenhum código novo

Build final, teste no navegador, README atualizado, vídeo de até 10 minutos, GitHub.

### Reserva

**Domingo 20** fica livre de propósito, para absorver o que atrasar.

## Ordem de corte, se a quinta chegar apertada

Da primeira coisa a sair para a última. Todas são folhas do plano: cortar qualquer
uma delas não obriga a refazer nada do que já estiver pronto.

1. **Rocket** - shotgun e metralhadora já provam que arma muda a forma do tiro
2. **Colete empilhável** - cai para um colete por vez
3. **Multiplicador de sequência** - o drop volta a ser 1 moeda fixa por abate
4. **Horda de Resistência** - as hordas viram todas de quota

Abaixo disso não dá para cortar sem perder pontos do enunciado.

## Arte pendente

O prisioneiro é um **quadrado amarelo** até a folha de sprite do POW ser recortada.
O placeholder é proposital: ele não bloqueia nenhum sistema, e trocar o sprite depois
é uma linha. Quando a arte entrar, creditar em `Fontes/CREDITOS.md` junto dos outros.
