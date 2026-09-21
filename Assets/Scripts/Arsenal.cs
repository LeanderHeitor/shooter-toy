using UnityEngine;

// As armas do jogo. Nenhuma mata mais que a outra - todo inimigo morre com um
// acerto, sempre. O que muda e a FORMA do tiro: quantos inimigos um disparo alcanca
// e com que rapidez os disparos saem.
public enum TipoDeArma
{
    Pistola,       // infinita, um toque um tiro. Nunca e comprada
    Shotgun,       // tres chumbos em leque: alcanca quem esta em alturas diferentes
    Metralhadora,  // segurar o botao repete o tiro
    Rocket         // explode no primeiro acerto e leva junto quem estiver perto
}

// Arsenal.cs
// O que o jogador carrega: a arma comprada, a municao dela e os coletes.
//
// Nao e um componente de proposito. Quem le isto sao o PlayerScript (para atirar e
// para morrer), a Loja (para vender) e o HUD (para mostrar), e nenhum dos tres e
// dono do assunto. Um componente pediria que alguem o arrastasse para a cena; uma
// classe static so precisa ser zerada no comeco da partida, e quem faz isso e o
// GameManager, junto com as moedas.
public static class Arsenal
{
    public static TipoDeArma arma = TipoDeArma.Pistola;
    public static int municao = 0;   // so vale para a arma comprada; a pistola e infinita
    public static int coletes = 0;

    // Mais que isto e comprar imortalidade em vez de uma segunda chance.
    public const int maximoDeColetes = 3;

    // Chamado pelo GameManager.Awake. A morte leva tudo: a partida nova comeca so
    // com a pistola, e e isso que faz cada compra da partida anterior ter doido.
    public static void Zerar()
    {
        arma = TipoDeArma.Pistola;
        municao = 0;
        coletes = 0;
    }

    // Troca de arma. A municao que sobrava da anterior se perde junto com ela: a
    // troca e, ela mesma, uma decisao cara, como no Metal Slug.
    public static void Equipar(TipoDeArma nova, int tiros)
    {
        arma = nova;
        municao = tiros;
    }

    public static void Recarregar(int tiros)
    {
        if (arma == TipoDeArma.Pistola) return;
        municao = municao + tiros;
    }

    // Chamado a cada disparo. Acabou a municao, volta a pistola sozinho: o jogador
    // nunca fica sem atirar, so volta a atirar do jeito mais fraco.
    public static void GastarTiro()
    {
        if (arma == TipoDeArma.Pistola) return;

        municao = municao - 1;
        if (municao <= 0) Equipar(TipoDeArma.Pistola, 0);
    }

    // Devolve true se um colete gastou o golpe no lugar do jogador.
    public static bool AbsorverGolpe()
    {
        if (coletes <= 0) return false;
        coletes = coletes - 1;
        return true;
    }

    // Nome para a tela. Fica aqui para a loja e o placar escreverem igual.
    public static string Nome(TipoDeArma tipo)
    {
        switch (tipo)
        {
            case TipoDeArma.Shotgun:      return "Shotgun";
            case TipoDeArma.Metralhadora: return "Metralhadora";
            case TipoDeArma.Rocket:       return "Rocket";
            default:                      return "Pistola";
        }
    }
}
