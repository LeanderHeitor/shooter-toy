# Fontes originais da arte

Esta pasta fica **fora de `Assets/`** de proposito. As folhas de personagem passam de
12.000 px de altura, e dentro de `Assets/` a Unity importaria e reduziria cada uma como
textura de jogo sem necessidade. O que o jogo realmente usa sao os quadros ja recortados
em `Assets/Art/Characters/` e as camadas em `Assets/Art/Background/`, gerados a partir
daqui.

## Personagens - Metal Slug 3

| Arquivo | Uso no jogo |
|---|---|
| `...Playable Characters - Tarma Roving.png` | Player: Idle, Run, Shoot, Death, Jump |
| `...Enemies & Bosses - Rebel Soldier.png` | Inimigo melee (Run com faca) e a morte compartilhada |
| `...Enemies & Bosses - Rebel Soldier (Rifle).png` | Inimigo atirador: Walk e Shoot |
| `...Non-Playable Characters - P.O.W.s _ Helpful NPCs.png` e `Metal Slug - POWs (Kodama).gif` | Prisioneiro da loja: amarrado (POW_Tied) e solto (POW_Idle) |
| `...Miscellaneous - Items.png` | Moeda girando (Coin_Spin) |
| `...Miscellaneous - Blood.gif` | Sangue do abate (Blood_Spray) |
| `Assets/Art/Characters/BOSS HORDER 5/...Rebel Soldier (Mini-Gun).png` | Chefe da horda 5: Minigun_Walk, Minigun_Aim e Minigun_Fire |
| `Assets/Art/Characters/BOSS HORDER 10/...Type 02 ''Di-Cokka'.png` | Chefe da horda 10: DiCokka_Drive, Aim, Fire e Wreck; o obus (DiCokka_Shell) e a fumaca das explosoes (Smoke_Explosion) |

**Credito exigido pelo proprio pacote**, conforme escrito dentro da folha do Rifle Soldier:

> CHARACTER: Rifle Soldier
> STYLE: "Metal Slug" series
> COPYRIGHTED BY: **SNK/Playmore**
> TILE-RIPPED BY: **Gussprint**
> REQUIREMENTS FOR USE: **Give credit.**

A folha do Di-Cokka pede o mesmo credito ao **Gussprint** ("give credit if used"). A folha
da Minigun traz a assinatura de **Goemar** e os enderecos `retrogamezone.co.uk` e
`metalslugsprites.co.uk` (Metal Slug Sprite Database).

Sprites originais de **Metal Slug 3**, propriedade da **SNK/Playmore**. Rips por **Gussprint** e **Goemar**.
Uso exclusivamente academico, sem fins comerciais, para a disciplina de Jogos Digitais.

## Cenario - NightForest

`NightForest.zip` traz o `.psd` em camadas mais os 6 PNGs separados usados no parallax.

Observacao tecnica: as camadas 1 e 4 nao costuravam horizontalmente (divergencia de 9,7% e
5,6% entre as bordas esquerda e direita), o que causaria emenda visivel no scroll infinito.
As versoes em `Assets/Art/Background/` sao **espelhadas** (`[A][espelho(A)]`), o que zera a
emenda por construcao. As camadas 3 e 5 sao raios de luz e nevoa: ficam presas na camera e
nao rolam, entao a emenda delas nao importa.
