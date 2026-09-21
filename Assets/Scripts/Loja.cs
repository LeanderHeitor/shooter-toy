using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // sistema de input NOVO da Unity (padrao deste projeto)

// Uma linha da tela da loja. So o que o HUD precisa para desenhar: quem decide o
// que acontece quando ela e comprada e a Loja.
public struct ItemDaLoja
{
    public string nome;
    public string detalhe;     // o que o dinheiro compra, ou o que se perde comprando
    public int preco;
    public bool indisponivel;  // colete no maximo, por exemplo
}

// Loja.cs
// O momento entre duas hordas em que as moedas viram armas e coletes.
//
// E aqui que a ganancia do jogo fecha o circulo: cada moeda que o jogador foi buscar
// no meio do tiroteio, ou que deixou de gastar numa granada, vira alguma coisa. E
// cada coisa comprada e dinheiro que nao sobra para a granada da proxima horda.
//
// Enquanto a loja esta aberta o mundo fica congelado, como na pausa. Comprar nao
// pode custar tempo: o custo da loja e o dinheiro, e so ele.
//
// Vai no mesmo objeto do GameManager, que a monta sozinho no Awake.
public class Loja : MonoBehaviour
{
    // ----- PRECOS -----
    // A municao custa o mesmo por tiro que a arma nova (ou um pouco mais). Refil e
    // conveniencia para quem esta pobre, nunca um atalho mais barato - senao o
    // jogador compra uma arma uma vez e ignora as outras para sempre.
    public int precoDoColete = 8;

    public int precoDaShotgun = 15;
    public int tirosDaShotgun = 20;
    public int precoDaMunicaoShotgun = 8;
    public int tirosDaMunicaoShotgun = 10;

    public int precoDaMetralhadora = 20;
    public int tirosDaMetralhadora = 60;
    public int precoDaMunicaoMetralhadora = 10;
    public int tirosDaMunicaoMetralhadora = 30;

    public int precoDoRocket = 30;
    public int tirosDoRocket = 8;
    public int precoDaMunicaoRocket = 15;
    public int tirosDaMunicaoRocket = 4;

    // Quanto tempo o aviso de compra fica na tela. Conta em tempo REAL: dentro da
    // loja o jogo esta congelado e o Time.time nao anda.
    public float duracaoDoAviso = 1.4f;

    // ----- PARA O HUD LER -----
    public static Loja atual;
    public static Prisioneiro prisioneiro;
    public static bool jogadorNoBalcao = false;   // perto o bastante para apertar E
    public static string aviso = "";
    public static bool avisoEhErro = false;
    public static float avisoAte = -1f;

    private static GameObject prefab;
    private Transform jogador;
    private PlayerScript jogadorScript;

    void Awake()
    {
        // Static sobrevive ao recarregamento da cena; o prisioneiro nao. Sem isto a
        // partida nova apontaria para um objeto ja destruido.
        atual = this;
        prisioneiro = null;
        jogadorNoBalcao = false;
        aviso = "";
        avisoAte = -1f;
    }

    void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            jogador = go.transform;
            jogadorScript = go.GetComponent<PlayerScript>();
        }
    }

    // Chamado pela Horda quando o cerco abre.
    public void ChamarPrisioneiro(Vector3 onde)
    {
        DispensarPrisioneiro();

        if (prefab == null) prefab = Resources.Load<GameObject>("Prisioneiro");
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, onde, Quaternion.identity);
        prisioneiro = go.GetComponent<Prisioneiro>();
    }

    // Chamado pela Horda quando a proxima comeca. A loja so existe com o cerco
    // aberto, nunca durante uma horda.
    public void DispensarPrisioneiro()
    {
        if (prisioneiro != null) Destroy(prisioneiro.gameObject);
        prisioneiro = null;
        jogadorNoBalcao = false;
    }

    void Update()
    {
        jogadorNoBalcao = PertoDoPrisioneiro();

        // Uma tecla, uma troca de tela: o E que abriu a loja neste quadro nao pode
        // ser lido de novo como "fechar". Ver GameManager.quadroDaUltimaTroca.
        if (Time.frameCount == GameManager.quadroDaUltimaTroca) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (GameManager.estado == Estado.Jogando)
        {
            if (jogadorNoBalcao && ApertouConversar(kb))
            {
                prisioneiro.solto = true;
                GameManager.IrPara(Estado.Loja);
            }
        }
        else if (GameManager.estado == Estado.Loja)
        {
            if (ApertouConversar(kb) || kb.escapeKey.wasPressedThisFrame)
            {
                GameManager.IrPara(Estado.Jogando);
                return;
            }

            int escolhido = TeclaDeCompra(kb);
            if (escolhido >= 0) Comprar(escolhido);
        }
    }

    bool PertoDoPrisioneiro()
    {
        if (prisioneiro == null || jogador == null) return false;
        if (jogadorScript != null && jogadorScript.isMorto) return false;

        bool perto = prisioneiro.Perto(jogador.position.x);

        // Chegar perto ja solta as cordas, mesmo sem abrir a loja: o jogador ve que
        // libertou alguem antes de saber que ele vende coisas.
        if (perto) prisioneiro.solto = true;
        return perto;
    }

    // E, ou a seta para cima / W, que e o que se aperta em frente a uma porta.
    static bool ApertouConversar(Keyboard kb)
    {
        return kb.eKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame ||
               kb.upArrowKey.wasPressedThisFrame;
    }

    // 1 a 4, na fileira de cima ou no teclado numerico. Devolve -1 se nada.
    static int TeclaDeCompra(Keyboard kb)
    {
        if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) return 0;
        if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) return 1;
        if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) return 2;
        if (kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame) return 3;
        return -1;
    }

    // A loja tem sempre quatro linhas: o colete e uma para cada arma. A linha da
    // arma que o jogador ja carrega vira a linha da municao dela - comprar a mesma
    // arma de novo nao faz sentido, e oito linhas na primeira visita seria ler
    // demais para quem tem 12 moedas.
    public List<ItemDaLoja> Itens()
    {
        List<ItemDaLoja> itens = new List<ItemDaLoja>();

        ItemDaLoja colete = new ItemDaLoja();
        colete.nome = "Colete";
        colete.preco = precoDoColete;
        colete.indisponivel = Arsenal.coletes >= Arsenal.maximoDeColetes;
        colete.detalhe = colete.indisponivel
            ? "já tem " + Arsenal.maximoDeColetes
            : "absorve 1 golpe   (tem " + Arsenal.coletes + ")";
        itens.Add(colete);

        itens.Add(LinhaDaArma(TipoDeArma.Shotgun, "3 tiros em leque"));
        itens.Add(LinhaDaArma(TipoDeArma.Metralhadora, "segure para atirar"));
        itens.Add(LinhaDaArma(TipoDeArma.Rocket, "explode em área"));

        return itens;
    }

    ItemDaLoja LinhaDaArma(TipoDeArma tipo, string descricao)
    {
        ItemDaLoja item = new ItemDaLoja();

        if (Arsenal.arma == tipo)
        {
            item.nome = "Munição " + Arsenal.Nome(tipo);
            item.preco = PrecoDaMunicao(tipo);
            item.detalhe = "+" + TirosDaMunicao(tipo) + " tiros";
            return item;
        }

        item.nome = Arsenal.Nome(tipo);
        item.preco = PrecoDaArma(tipo);
        item.detalhe = descricao + ", " + TirosDaArma(tipo) + " tiros";

        // Avisar o que se perde e o que faz a troca de arma ser uma decisao, e nao
        // um clique. Sem isto o jogador so descobre depois, e acha que foi roubado.
        if (Arsenal.arma != TipoDeArma.Pistola)
        {
            item.detalhe = item.detalhe + "   (perde " + Arsenal.municao + " de " +
                           Arsenal.Nome(Arsenal.arma) + ")";
        }

        return item;
    }

    void Comprar(int indice)
    {
        List<ItemDaLoja> itens = Itens();
        if (indice < 0 || indice >= itens.Count) return;

        ItemDaLoja item = itens[indice];

        if (item.indisponivel)
        {
            Avisar(item.detalhe, true);
            return;
        }

        // Pergunta antes de entregar: o GameManager so cobra se houver saldo.
        if (GameManager.Gastar(item.preco) == false)
        {
            int falta = item.preco - GameManager.moedas;
            Avisar("faltam " + falta + (falta == 1 ? " moeda" : " moedas"), true);
            return;
        }

        if (indice == 0)
        {
            Arsenal.coletes = Arsenal.coletes + 1;
        }
        else
        {
            TipoDeArma tipo = (TipoDeArma)indice;   // 1 Shotgun, 2 Metralhadora, 3 Rocket

            if (Arsenal.arma == tipo) Arsenal.Recarregar(TirosDaMunicao(tipo));
            else Arsenal.Equipar(tipo, TirosDaArma(tipo));
        }

        Avisar(item.nome + " comprado!", false);
    }

    void Avisar(string texto, bool ehErro)
    {
        aviso = texto;
        avisoEhErro = ehErro;
        avisoAte = Time.unscaledTime + duracaoDoAviso;
    }

    int PrecoDaArma(TipoDeArma t)
    {
        if (t == TipoDeArma.Shotgun) return precoDaShotgun;
        if (t == TipoDeArma.Metralhadora) return precoDaMetralhadora;
        return precoDoRocket;
    }

    int TirosDaArma(TipoDeArma t)
    {
        if (t == TipoDeArma.Shotgun) return tirosDaShotgun;
        if (t == TipoDeArma.Metralhadora) return tirosDaMetralhadora;
        return tirosDoRocket;
    }

    int PrecoDaMunicao(TipoDeArma t)
    {
        if (t == TipoDeArma.Shotgun) return precoDaMunicaoShotgun;
        if (t == TipoDeArma.Metralhadora) return precoDaMunicaoMetralhadora;
        return precoDaMunicaoRocket;
    }

    int TirosDaMunicao(TipoDeArma t)
    {
        if (t == TipoDeArma.Shotgun) return tirosDaMunicaoShotgun;
        if (t == TipoDeArma.Metralhadora) return tirosDaMunicaoMetralhadora;
        return tirosDaMunicaoRocket;
    }
}
