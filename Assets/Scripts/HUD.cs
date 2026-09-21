using UnityEngine;

// HUD.cs
// Desenha TODAS as telas do jogo: menu de entrada, placar, pausa, loja e fim de jogo.
// Nao existe Canvas nem cena separada - e tudo OnGUI por cima do mesmo mundo, e
// quem decide qual tela aparece e o GameManager.estado.
//
// A fonte vem de um ASSET do projeto (Assets/Fonts/ComicSansMS.ttf), nao das fontes
// instaladas no Windows: no WebGL nao existe fonte de sistema, e o
// Font.CreateDynamicFontFromOSFont que estava aqui devolvia nulo no navegador,
// fazendo TODO o texto do jogo sumir do build publico.
// O tamanho da letra sai de Screen.height, senao em tela grande o texto fica minusculo.
public class HUD : MonoBehaviour
{
    // A altura da tela dividida por isto vira o tamanho da letra.
    // Em 1080p da 45 pixels; numero menor = letra maior.
    public int divisorDaLetra = 24;

    // As telas centrais usam uma letra maior que a do placar.
    public float escalaDoFimDeJogo = 1.8f;

    // Arraste Assets/Fonts/ComicSansMS.ttf aqui no Inspector.
    public Font fonte;

    // O nome que aparece na tela de entrada. Trocar aqui nao mexe em mais nada.
    public string titulo = "SHOOTER TOY";

    // O mesmo ouro do sprite da moeda, para o numero no canto e o objeto no chao
    // serem lidos como a mesma coisa sem ninguem precisar explicar. A sequencia usa
    // este mesmo ouro porque ela tambem e dinheiro: o que ela faz e pagar mais moeda
    // por abate.
    private static readonly Color corDaMoeda = new Color(1f, 0.82f, 0.29f);

    // O ouro das telas: titulo e recorde. E um destaque de tela parada, nao um valor
    // em disputa, e por isso nao e a mesma cor da moeda.
    private static readonly Color corDeDestaque = new Color(1f, 0.85f, 0.25f);

    // Sprites das telas finais, carregados uma vez do Resources. O OnGUI roda
    // varias vezes por quadro, e carregar prefab ali dentro seria jogar tempo fora.
    private Sprite[] quadrosDoPrisioneiro;
    private string chefeCarregado;
    private Sprite silhuetaDoChefe;
    private string nomeDoChefe;

    private GUIStyle placar;
    private GUIStyle grande;
    private GUIStyle dica;
    private int tamanhoUsado = -1;
    private bool jaAvisouDaFonte = false;

    // NAO tem OnDestroy destruindo a fonte: agora ela e um asset do projeto, e
    // Destroy num asset apagaria o arquivo da pasta, nao uma copia em memoria.

    // Monta os estilos uma vez so, e refaz se a janela mudar de tamanho.
    void PrepararEstilos(int tamanho)
    {
        if (tamanhoUsado == tamanho) return;
        tamanhoUsado = tamanho;

        if (fonte == null && jaAvisouDaFonte == false)
        {
            jaAvisouDaFonte = true;
            Debug.LogWarning("HUD: a fonte nao foi arrastada no Inspector. " +
                             "Arraste Assets/Fonts/ComicSansMS.ttf no campo Fonte.");
        }

        placar = new GUIStyle(GUI.skin.label);
        // Com a fonte nula o GUIStyle usa a fonte padrao da Unity: feio, mas o texto
        // aparece. Sumir da tela seria pior do que aparecer errado.
        if (fonte != null) placar.font = fonte; // grande e dica herdam a mesma Comic Sans
        placar.fontSize = tamanho;
        placar.fontStyle = FontStyle.Bold;
        placar.normal.textColor = Color.white;

        // O GUI.skin.label de onde este estilo herda vem com clipping = Clip: o que
        // nao couber no retangulo e CORTADO. Como os retangulos daqui sao calculados
        // a partir do tamanho da letra, qualquer erro de conta virava letra cortada
        // pela metade - e a Comic Sans, de ascendente alta e descendente funda, erra
        // essa conta com facilidade. Com Overflow o retangulo passa a ser so um ponto
        // de referencia para o alinhamento, e o texto desenha inteiro sempre.
        placar.clipping = TextClipping.Overflow;
        // Mesma ideia: sem isto uma frase pode quebrar em duas linhas dentro de um
        // retangulo estreito em vez de sair em linha unica.
        placar.wordWrap = false;

        grande = new GUIStyle(placar);
        grande.fontSize = Mathf.RoundToInt(tamanho * escalaDoFimDeJogo);
        grande.alignment = TextAnchor.MiddleCenter;

        // Mesmo tamanho do placar, mas centralizado: e a linha de instrucao das telas.
        dica = new GUIStyle(placar);
        dica.fontStyle = FontStyle.Normal;
        dica.alignment = TextAnchor.MiddleCenter;
    }

    // Escreve o texto quatro vezes em preto, deslocado, e uma vez em branco por cima.
    // Sem isso o texto branco some nas partes claras do fundo da floresta.
    void Escrever(Rect onde, string texto, GUIStyle estilo, Color cor)
    {
        float d = Mathf.Max(1f, estilo.fontSize * 0.06f);

        Color guardada = estilo.normal.textColor;
        estilo.normal.textColor = Color.black;
        estilo.Draw(new Rect(onde.x - d, onde.y, onde.width, onde.height), texto, false, false, false, false);
        estilo.Draw(new Rect(onde.x + d, onde.y, onde.width, onde.height), texto, false, false, false, false);
        estilo.Draw(new Rect(onde.x, onde.y - d, onde.width, onde.height), texto, false, false, false, false);
        estilo.Draw(new Rect(onde.x, onde.y + d, onde.width, onde.height), texto, false, false, false, false);

        estilo.normal.textColor = cor;
        estilo.Draw(onde, texto, false, false, false, false);
        estilo.normal.textColor = guardada;
    }

    // Uma faixa da largura da tela, centrada verticalmente em "centroY".
    // Todo estilo centralizado desenhado aqui e MiddleCenter, entao alargar a faixa
    // NAO move o texto: ele continua centrado no mesmo ponto. A folga so existe para
    // dar espaco de sobra ao redor da letra.
    Rect Faixa(float centroY, float altura)
    {
        return new Rect(0f, centroY - (altura * 0.5f), Screen.width, altura);
    }

    // Escurece a tela inteira. A cor muda conforme a tela para que o jogador saiba
    // onde esta sem ler nada: vermelho e morte, azul e pausa ou menu.
    void Veu(Color cor)
    {
        Color guardada = GUI.color;
        GUI.color = cor;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = guardada;
    }

    void OnGUI()
    {
        // Desenha texto sem herdar o destaque de mouse dos labels do tema da Unity.
        if (Event.current.type != EventType.Repaint) return;

        int tamanho = Mathf.Max(14, Screen.height / divisorDaLetra);
        PrepararEstilos(tamanho);

        // O clarao vem antes de tudo, por baixo dos textos: o placar continua legivel
        // no instante da explosao, que e quando o jogador olha o saldo que acabou de cair.
        DesenharClarao();

        // As telas centrais vem ANTES do placar, para que o placar continue legivel
        // por cima do veu em vez de sumir atras dele.
        switch (GameManager.estado)
        {
            case Estado.Menu:      DesenharMenu(tamanho);      break;
            case Estado.Pausado:   DesenharPausa(tamanho);     break;
            case Estado.Loja:      DesenharLoja(tamanho);      break;
            case Estado.FimDeJogo: DesenharFimDeJogo(tamanho); break;
            case Estado.Vitoria:   DesenharVitoria(tamanho);   break;
            case Estado.Metade:    DesenharMetade(tamanho);    break;
        }

        // No menu o placar nao aparece: nao ha partida acontecendo, e um "Abates: 0"
        // no canto da tela de titulo so polui.
        if (GameManager.estado != Estado.Menu)
        {
            DesenharPlacar(tamanho);
            DesenharArsenal(tamanho);
            if (GameManager.estado != Estado.Loja && GameManager.estado != Estado.Vitoria &&
                GameManager.estado != Estado.Metade)
                DesenharHorda(tamanho);
        }

        if (GameManager.estado == Estado.Jogando) DesenharBalcao(tamanho);
    }

    // O canto superior direito: o que o jogador carrega. Fica longe das moedas de
    // proposito - moeda e o que ele TEM para gastar, isto e o que ja gastou.
    void DesenharArsenal(int tamanho)
    {
        float linha = tamanho * 1.35f;
        float margem = tamanho * 0.6f;

        GUIStyle direita = new GUIStyle(placar);
        direita.alignment = TextAnchor.UpperRight;
        Rect Linha(int n) => new Rect(0f, margem + (linha * n), Screen.width - margem, linha);

        // A municao aparece sempre junto do nome: e ela que diz quando a arma vai
        // voltar a ser a pistola, e o jogador precisa ver isso chegando.
        string arma = (Arsenal.arma == TipoDeArma.Pistola)
            ? "Pistola"
            : Arsenal.Nome(Arsenal.arma) + ": " + Arsenal.municao;
        Color corDaArma = (Arsenal.arma != TipoDeArma.Pistola && Arsenal.municao <= 5)
            ? new Color(1f, 0.4f, 0.35f) : Color.white;
        Escrever(Linha(0), arma, direita, corDaArma);

        if (Arsenal.coletes > 0)
        {
            string texto = (Arsenal.coletes == 1) ? "1 colete" : Arsenal.coletes + " coletes";
            Escrever(Linha(1), texto, direita, new Color(0.55f, 0.8f, 1f));
        }

        if (Arsenal.segundaChance)
        {
            int n = (Arsenal.coletes > 0) ? 2 : 1;
            Escrever(Linha(n), "segunda chance", direita, corDaMoeda);
        }
    }

    // O "E" em cima da cabeca do prisioneiro, so quando o jogador esta perto o
    // bastante para ele funcionar. Aparecer de longe ensinaria que o E funciona de
    // qualquer lugar.
    void DesenharBalcao(int tamanho)
    {
        if (Loja.jogadorNoBalcao == false || Loja.prisioneiro == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Um pouco acima da cabeca dele. O OnGUI conta o Y de cima para baixo e a
        // camera de baixo para cima, dai a inversao.
        Vector3 tela = cam.WorldToScreenPoint(Loja.prisioneiro.transform.position + new Vector3(0f, 1.6f, 0f));
        float y = Screen.height - tela.y;

        Rect onde = new Rect(tela.x - Screen.width * 0.5f, y - tamanho, Screen.width, tamanho * 2f);
        Escrever(onde, "E  loja", dica, corDaMoeda);
    }

    // A tela da loja. O mundo fica congelado atras, como na pausa, e o placar
    // continua no canto: e o saldo que o jogador olha antes de cada compra.
    void DesenharLoja(int tamanho)
    {
        Veu(new Color(0.08f, 0.06f, 0.02f, 0.80f));

        Loja loja = Loja.atual;
        if (loja == null) return;

        float linha = tamanho * 1.6f;
        float topo = Screen.height * 0.26f;

        Escrever(Faixa(topo, grande.fontSize * 2f), "LOJA", grande, corDeDestaque);

        System.Collections.Generic.List<ItemDaLoja> itens = loja.Itens();
        float esquerda = Screen.width * 0.18f;
        float largura = Screen.width * 0.64f;

        GUIStyle direita = new GUIStyle(placar);
        direita.alignment = TextAnchor.UpperRight;
        GUIStyle detalhe = new GUIStyle(placar);
        detalhe.fontStyle = FontStyle.Normal;
        detalhe.fontSize = Mathf.RoundToInt(tamanho * 0.7f);

        for (int i = 0; i < itens.Count; i++)
        {
            ItemDaLoja item = itens[i];
            float y = topo + (linha * 1.6f) + (linha * 1.35f * i);

            // Cinza quando nao da para comprar: o jogador ve de relance o que o
            // saldo alcanca, sem fazer conta. E a ganancia desenhada na tela.
            bool podePagar = GameManager.moedas >= item.preco && item.indisponivel == false;
            Color cor = podePagar ? Color.white : new Color(0.55f, 0.55f, 0.55f);

            Escrever(new Rect(esquerda, y, largura, linha), (i + 1) + "   " + item.nome, placar, cor);
            Escrever(new Rect(esquerda, y, largura, linha), item.preco + " moedas", direita,
                     podePagar ? corDaMoeda : cor);
            Escrever(new Rect(esquerda + tamanho * 1.6f, y + tamanho * 1.05f, largura, linha),
                     item.detalhe, detalhe, new Color(0.75f, 0.75f, 0.75f));
        }

        float rodape = topo + (linha * 1.6f) + (linha * 1.35f * itens.Count) + linha * 0.6f;

        if (Time.unscaledTime < Loja.avisoAte)
        {
            Color corDoAviso = Loja.avisoEhErro ? new Color(1f, 0.4f, 0.35f) : new Color(0.65f, 0.95f, 0.7f);
            Escrever(Faixa(rodape, dica.fontSize * 2f), Loja.aviso, dica, corDoAviso);
        }

        Escrever(Faixa(rodape + linha * 1.1f, dica.fontSize * 2f),
                 "1-5 comprar   ·   E ou Esc sair", dica, new Color(0.8f, 0.8f, 0.75f));
    }

    // O topo da tela, no centro. A horda fica aqui, e nao no canto com o resto do
    // placar, porque ela nao e um numero que o jogador consulta quando sobra tempo:
    // e o objetivo imediato dele. No meio do tiroteio o olho ja esta perto do centro.
    void DesenharHorda(int tamanho)
    {
        string texto;
        Color cor;

        if (Horda.emCombate == false)
        {
            // Entre hordas o texto deixa de informar e passa a pedir: e o unico
            // momento em que o jogo diz ao jogador o que fazer para continuar.
            texto = (Horda.numero == 0)
                ? "siga em frente para a primeira horda"
                : "Horda " + Horda.numero + " limpa   ·   +" + Horda.ultimoBonus +
                  " moedas   ·   o prisioneiro tem uma loja";

            // O primeiro chefe caiu: o jogador precisa saber que o jogo tem fim, e
            // que ja andou metade dele. Um objetivo visivel puxa mais que um infinito.
            if (Horda.numero == Horda.hordaDaVitoria / 2)
                texto = "METADE DO CAMINHO!   ·   " + texto;
            cor = new Color(0.65f, 0.95f, 0.7f);
        }
        else if (Horda.tipo == TipoDeHorda.Resistencia && Chefe.atual != null)
        {
            DesenharBarraDoChefe(tamanho);
            return;
        }
        else if (Horda.tipo == TipoDeHorda.Resistencia)
        {
            // O tempo aparece arredondado para CIMA: com o arredondamento normal o
            // cronometro mostraria "0s" por meio segundo ainda com inimigo em cima
            // do jogador, e ele leria isso como o jogo tendo travado.
            texto = "RESISTA   ·   " + Mathf.CeilToInt(Horda.segundosRestantes) + "s";
            cor = new Color(1f, 0.35f, 0.3f);
        }
        else
        {
            texto = "Horda " + Horda.numero + "   ·   restam " + Mathf.Max(0, Horda.restam);
            cor = Color.white;
        }

        Escrever(Faixa(tamanho * 1.1f, dica.fontSize * 2f), texto, dica, cor);
    }

    // ----- BARRA DO CHEFE -----
    // A unica barra de vida do jogo, e por isso ela e grande e fica no centro do
    // topo: e o objetivo inteiro da luta. O jogador nunca tem barra - ele continua
    // morrendo com um golpe - e o contraste e parte da tensao.
    private Chefe chefeDaBarra;
    private float barraApareceuEm;
    private float vidaVista = 1f;
    private float rastro = 0f;
    private float golpeEm = -1f;

    void DesenharBarraDoChefe(int tamanho)
    {
        Chefe c = Chefe.atual;

        if (c != chefeDaBarra)
        {
            chefeDaBarra = c;
            barraApareceuEm = Time.time;
            vidaVista = 1f;
            rastro = 0f;
            golpeEm = -1f;
        }

        float vida = c.FracaoDaVida();

        // Na entrada a barra enche do zero, como nos fliperamas: o jogador ve o
        // tamanho da luta sendo anunciado antes do primeiro tiro.
        float enche = Mathf.Clamp01((Time.time - barraApareceuEm) / 1.2f);
        float mostrada = Mathf.Min(vida, enche);

        if (vida < vidaVista - 0.0001f) golpeEm = Time.time;
        vidaVista = vida;

        // O rastro amarelo mostra quanto o ultimo golpe tirou. Ele espera um
        // instante e so entao desce: e o que faz um tiro de shotgun, com tres
        // chumbos entrando juntos, parecer maior que um tiro de pistola.
        if (rastro < mostrada) rastro = mostrada;
        else if (Time.time - golpeEm > 0.45f)
            rastro = Mathf.MoveTowards(rastro, mostrada, Time.deltaTime * 0.7f);

        // O nome no lugar do texto da horda.
        string titulo = c.EstaMorto()
            ? c.nome + " derrotado!"
            : "Horda " + Horda.numero + "   ·   " + c.nome;
        Color corDoTitulo = c.EstaMorto() ? corDeDestaque : new Color(1f, 0.45f, 0.35f);
        Escrever(Faixa(tamanho * 1.1f, dica.fontSize * 2f), titulo, dica, corDoTitulo);

        float largura = Screen.width * 0.55f;
        float altura = Mathf.Max(10f, tamanho * 0.7f);
        float x = (Screen.width - largura) * 0.5f;
        float y = tamanho * 2.2f;
        float borda = Mathf.Max(2f, tamanho * 0.08f);

        Retangulo(new Rect(x - borda, y - borda, largura + (borda * 2f), altura + (borda * 2f)), Color.black);
        Retangulo(new Rect(x, y, largura, altura), new Color(0.22f, 0.05f, 0.05f, 0.95f));
        Retangulo(new Rect(x, y, largura * rastro, altura), new Color(1f, 0.85f, 0.35f));

        // Abaixo da metade o chefe descansa menos entre ataques (ver Chefe), e a
        // barra pulsa para contar isso sem texto nenhum.
        Color corDaVida = new Color(0.85f, 0.12f, 0.1f);
        if (vida <= 0.5f && vida > 0f)
            corDaVida = Color.Lerp(corDaVida, new Color(1f, 0.45f, 0.1f), 0.5f + 0.5f * Mathf.Sin(Time.time * 10f));
        if (Time.time - golpeEm < 0.07f) corDaVida = Color.white;

        Retangulo(new Rect(x, y, largura * mostrada, altura), corDaVida);
        // Um brilho no terco de cima, para a barra ter volume em vez de ser um risco.
        Retangulo(new Rect(x, y, largura * mostrada, altura * 0.3f), new Color(1f, 1f, 1f, 0.25f));

        // Marcas a cada 10%: dao escala a barra, e o jogador consegue contar
        // quantos pedacos ainda faltam.
        for (int i = 1; i < 10; i++)
        {
            float tx = x + (largura * i / 10f);
            Retangulo(new Rect(tx - 1f, y, 2f, altura), new Color(0f, 0f, 0f, 0.45f));
        }
    }

    void Retangulo(Rect onde, Color cor)
    {
        Color guardada = GUI.color;
        GUI.color = cor;
        GUI.DrawTexture(onde, Texture2D.whiteTexture);
        GUI.color = guardada;
    }

    // Branco que some em 0,25s. Com o tempo congelado (pausa logo depois da granada)
    // o clarao fica parado junto com o mundo, que e o que se espera de uma pausa.
    void DesenharClarao()
    {
        float falta = Granada.claraoAte - Time.time;
        if (falta <= 0f) return;

        float forca = Mathf.Clamp01(falta / Granada.duracaoDoClarao);
        Veu(new Color(1f, 1f, 0.92f, 0.85f * forca));
    }

    // O canto superior esquerdo durante a partida.
    void DesenharPlacar(int tamanho)
    {
        float linha = tamanho * 1.35f;
        float margem = tamanho * 0.6f;
        float largura = Screen.width;

        Escrever(new Rect(margem, margem, largura, linha),
                 "Abates: " + GameManager.abates, placar, Color.white);

        Escrever(new Rect(margem, margem + linha, largura, linha),
                 "Recorde: " + GameManager.recorde, placar, Color.white);

        // As moedas ficam SEMPRE na tela, mesmo zeradas: o jogador precisa saber o
        // saldo a todo instante para decidir se gasta a granada agora ou guarda para
        // a loja. Um numero que some quando chega a zero esconde justamente o momento
        // em que a decisao fica mais dura.
        Escrever(new Rect(margem, margem + (linha * 2f), largura, linha),
                 "Moedas: " + GameManager.moedas + "   ·   granada " + Granada.custoAgora,
                 placar, corDaMoeda);

        // A sequencia so aparece quando esta valendo alguma coisa. Ela sumir da tela
        // ja e o aviso de que voce acabou de perde-la.
        if (GameManager.sequencia >= 2)
        {
            // O bonus aparece junto do numero porque, sem ele, o rebelde da faca
            // soltando duas moedas parecia defeito. Visivel, vira motivo para o
            // jogador nao deixar a sequencia morrer.
            string texto = "Sequência: " + GameManager.sequencia;
            int bonus = GameManager.BonusDeSequencia();
            if (bonus > 0) texto = texto + "  ·  +" + bonus + (bonus == 1 ? " moeda" : " moedas");

            Escrever(new Rect(margem, margem + (linha * 3f), largura, linha),
                     texto, placar, corDaMoeda);
        }

        // Aparece embaixo do saldo, que e para onde o olho vai quando o K nao funciona.
        if (Time.time < Granada.avisoDeSaldoAte)
        {
            int falta = Granada.faltouNoAviso;
            Escrever(new Rect(margem, margem + (linha * 4f), largura, linha),
                     "Granada: faltam " + falta + (falta == 1 ? " moeda" : " moedas"),
                     placar, new Color(1f, 0.4f, 0.35f));
        }
    }

    // Tela de entrada. O mundo ja esta montado atras dela, so congelado.
    void DesenharMenu(int tamanho)
    {
        Veu(new Color(0.03f, 0.05f, 0.10f, 0.78f));

        float linha = tamanho * 1.5f;
        float meio = Screen.height * 0.5f;

        Escrever(Faixa(meio - (linha * 1.4f), grande.fontSize * 2f),
                 titulo, grande, corDeDestaque);

        Escrever(Faixa(meio + (linha * 0.3f), dica.fontSize * 2f),
                 "aperte qualquer tecla para começar", dica, Color.white);

        // O recorde so aparece depois da primeira partida - antes dele existir,
        // "Recorde: 0" nao informa nada e ocupa espaco.
        if (GameManager.recorde > 0)
        {
            // A horda entra no recorde junto dos abates porque as duas medem coisas
            // diferentes: da para morrer com muitos abates numa horda baixa, ou com
            // poucos numa horda alta. Uma so das duas contaria meia historia.
            string texto = "Recorde: " + GameManager.recorde + " abates";
            if (GameManager.recordeDeHorda > 0)
            {
                texto = texto + "   ·   horda " + GameManager.recordeDeHorda;
            }
            if (GameManager.recordeDeMoedas > 0)
            {
                texto = texto + "   ·   vitória mais rica: " + GameManager.recordeDeMoedas + " moedas";
            }

            Escrever(Faixa(meio + (linha * 1.5f), dica.fontSize * 2f),
                     texto, dica, corDeDestaque);
        }

        // Os controles no rodape: e a unica tela onde o jogador tem tempo de ler.
        Escrever(Faixa(Screen.height - (linha * 1.1f), dica.fontSize * 2f),
                 "A D mover   ·   ESPAÇO pular   ·   J atirar   ·   K granada   ·   E loja   ·   P pausa",
                 dica, new Color(0.75f, 0.78f, 0.85f));
    }

    // Pausa: mesmo veu azul do menu, para o jogador ler que nao morreu.
    void DesenharPausa(int tamanho)
    {
        Veu(new Color(0.03f, 0.05f, 0.10f, 0.72f));

        float linha = tamanho * 1.5f;
        float meio = Screen.height * 0.5f;

        Escrever(Faixa(meio - (linha * 0.4f), grande.fontSize * 2f),
                 "PAUSADO", grande, Color.white);

        Escrever(Faixa(meio + (linha * 1.1f), dica.fontSize * 2f),
                 "Esc ou P para voltar", dica, new Color(0.8f, 0.83f, 0.9f));
    }

    // Tela de fim de jogo: simples de proposito. Um veu vermelho e uma frase.
    // Os numeros continuam legiveis no canto, no placar normal, entao repeti-los aqui
    // so encheria a tela.
    void DesenharFimDeJogo(int tamanho)
    {
        Veu(new Color(0.30f, 0.02f, 0.03f, 0.78f));

        float linha = tamanho * 1.5f;
        float meio = (Screen.height * 0.5f) - (linha * 2.5f);

        float centroDoTitulo = meio + (linha * 0.5f);
        Escrever(Faixa(centroDoTitulo, grande.fontSize * 2f),
                 "VOCÊ MORREU", grande, new Color(1f, 0.25f, 0.2f));

        // A comparacao com o recorde e o gancho do "so mais uma": "faltaram 3" doi
        // mais que qualquer numero solto, porque diz que era possivel.
        int antes = GameManager.recordeAntesDaPartida;
        string comparacao;
        Color corDaComparacao = corDeDestaque;
        if (antes == 0)
            comparacao = "Seu primeiro recorde: " + GameManager.abates + " abates";
        else if (GameManager.abates > antes)
            comparacao = "NOVO RECORDE!  " + GameManager.abates + " abates";
        else
        {
            int faltaram = antes - GameManager.abates + 1;
            comparacao = "Faltaram " + faltaram + (faltaram == 1 ? " abate" : " abates") + " para o recorde";
            corDaComparacao = Color.white;
        }
        Escrever(Faixa(meio + (linha * 1.9f), dica.fontSize * 2f), comparacao, dica, corDaComparacao);

        DesenharQuemEspera(tamanho, meio + (linha * 4.4f), Horda.numero);

        string textoDica = "aperte qualquer tecla";
        float centroDaDica = meio + (linha * 7.3f);
        Rect areaDica = Faixa(centroDaDica, dica.fontSize * 2f);

        // O desenho usa a tela toda, mas o mouse so deve acender a frase quando
        // estiver em cima DELA - por isso o alvo do hover continua estreito.
        // Medir aqui e seguro: errar a medida por alguns pixels muda onde o mouse
        // acende, nunca o que aparece na tela.
        float larguraDica = dica.CalcSize(new GUIContent(textoDica)).x;
        Rect alvoDoMouse = new Rect((Screen.width - larguraDica) * 0.5f,
                                    centroDaDica - (linha * 0.5f), larguraDica, linha);

        Color corDica = alvoDoMouse.Contains(Event.current.mousePosition)
            ? Color.white : new Color(0.9f, 0.6f, 0.6f);
        Escrever(areaDica, textoDica, dica, corDica);
    }

    // A silhueta preta do proximo chefe que o jogador ainda nao derrubou. Mostrar so
    // a sombra, e nao o desenho, e a provocacao: da vontade de ver quem e.
    void DesenharQuemEspera(int tamanho, float centroY, int aPartirDaHorda)
    {
        Horda horda = FindAnyObjectByType<Horda>();
        if (horda == null) return;

        int hordaDoChefe = horda.HordaDoProximoChefe(aPartirDaHorda);
        string prefab = horda.PrefabDoChefe(hordaDoChefe);
        if (prefab != chefeCarregado)
        {
            chefeCarregado = prefab;
            silhuetaDoChefe = null;
            nomeDoChefe = null;

            GameObject go = (prefab != null) ? Resources.Load<GameObject>(prefab) : null;
            Chefe c = (go != null) ? go.GetComponent<Chefe>() : null;
            if (c != null)
            {
                nomeDoChefe = c.nome;
                if (c.quadrosAndando != null && c.quadrosAndando.Length > 0)
                    silhuetaDoChefe = c.quadrosAndando[0];
            }
        }
        if (nomeDoChefe == null) return;

        float altura = tamanho * 4f;
        DesenharSprite(silhuetaDoChefe, new Vector2(Screen.width * 0.5f, centroY), altura, Color.black);

        Escrever(Faixa(centroY + altura * 0.5f + tamanho * 0.7f, dica.fontSize * 2f),
                 "Horda " + hordaDoChefe + ": o " + nomeDoChefe + " te espera",
                 dica, new Color(0.9f, 0.6f, 0.6f));
    }

    // Desenha um sprite solto no OnGUI, centrado no ponto pedido. Usado pelas telas
    // finais, que ficam por cima de um mundo congelado e nao podem contar com um
    // SpriteRenderer animando.
    void DesenharSprite(Sprite sprite, Vector2 centro, float altura, Color cor)
    {
        if (sprite == null) return;

        Texture2D tex = sprite.texture;
        Rect r = sprite.textureRect;
        Rect coords = new Rect(r.x / tex.width, r.y / tex.height, r.width / tex.width, r.height / tex.height);

        float largura = altura * (r.width / r.height);
        Rect onde = new Rect(centro.x - largura * 0.5f, centro.y - altura * 0.5f, largura, altura);

        Color guardada = GUI.color;
        GUI.color = cor;
        GUI.DrawTextureWithTexCoords(onde, tex, coords);
        GUI.color = guardada;
    }

    // O respiro depois do primeiro chefe. Mesmo veu azul da pausa, porque e isso que
    // ela e: o jogo parado, a partida intacta. A silhueta do tanque mostra que o
    // "quase la" ainda tem um tamanho.
    void DesenharMetade(int tamanho)
    {
        Veu(new Color(0.03f, 0.05f, 0.10f, 0.78f));

        float linha = tamanho * 1.5f;
        float meio = (Screen.height * 0.5f) - (linha * 2.5f);

        Escrever(Faixa(meio + (linha * 0.5f), grande.fontSize * 2f),
                 "VOCÊ ESTÁ QUASE LÁ", grande, corDeDestaque);
        Escrever(Faixa(meio + (linha * 1.9f), dica.fontSize * 2f),
                 "mas existem mais perigos pela frente", dica, Color.white);

        DesenharQuemEspera(tamanho, meio + (linha * 4.4f), Horda.numero + 1);

        Escrever(Faixa(meio + (linha * 7.3f), dica.fontSize * 2f),
                 "aperte qualquer tecla para continuar", dica, new Color(0.8f, 0.83f, 0.9f));
    }

    // A vitoria. O veu e dourado, a cor da moeda: e o dinheiro guardado que vira
    // placar aqui. O prisioneiro aparece solto ao lado do texto porque ele e o
    // motivo da vitoria: o unico personagem que nao queria te matar esta livre.
    void DesenharVitoria(int tamanho)
    {
        Veu(new Color(0.25f, 0.18f, 0.02f, 0.82f));

        float linha = tamanho * 1.5f;
        float meio = (Screen.height * 0.5f) - linha * 2f;

        Escrever(Faixa(meio, grande.fontSize * 2f), "você ganhou parabens", grande, corDeDestaque);

        if (quadrosDoPrisioneiro == null)
        {
            GameObject go = Resources.Load<GameObject>("Prisioneiro");
            Prisioneiro p = (go != null) ? go.GetComponent<Prisioneiro>() : null;
            quadrosDoPrisioneiro = (p != null && p.quadrosSolto != null) ? p.quadrosSolto : new Sprite[0];
        }

        // O mundo esta congelado, entao a animacao anda pelo relogio real.
        if (quadrosDoPrisioneiro.Length > 0)
        {
            int quadro = (int)(Time.unscaledTime * 8f) % quadrosDoPrisioneiro.Length;
            // Ele pula de alegria: um seno no relogio real, sem precisar de sprite novo.
            float pulo = Mathf.Abs(Mathf.Sin(Time.unscaledTime * 6f)) * tamanho * 0.8f;
            DesenharSprite(quadrosDoPrisioneiro[quadro],
                           new Vector2(Screen.width * 0.5f, meio + linha * 2.3f - pulo),
                           tamanho * 3f, Color.white);
        }

        Escrever(Faixa(meio + linha * 4.1f, dica.fontSize * 2f),
                 "o prisioneiro está livre   ·   " + GameManager.abates + " abates",
                 dica, Color.white);

        string moedas = GameManager.bateuRecordeDeMoedas
            ? "NOVO RECORDE!  vitória com " + GameManager.moedas + " moedas guardadas"
            : "vitória com " + GameManager.moedas + " moedas   ·   a mais rica: " + GameManager.recordeDeMoedas;
        Escrever(Faixa(meio + linha * 5.1f, dica.fontSize * 2f), moedas, dica, corDaMoeda);

        Escrever(Faixa(meio + linha * 6.6f, dica.fontSize * 2f),
                 "aperte qualquer tecla", dica, new Color(0.95f, 0.9f, 0.7f));
    }
}
