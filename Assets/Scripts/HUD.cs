using UnityEngine;

// HUD.cs
// Desenha TODAS as telas do jogo: menu de entrada, placar, pausa e fim de jogo.
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
            case Estado.FimDeJogo: DesenharFimDeJogo(tamanho); break;
        }

        // No menu o placar nao aparece: nao ha partida acontecendo, e um "Abates: 0"
        // no canto da tela de titulo so polui.
        if (GameManager.estado != Estado.Menu)
        {
            DesenharPlacar(tamanho);
            DesenharHorda(tamanho);
        }
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
                  " moedas   ·   siga em frente";
            cor = new Color(0.65f, 0.95f, 0.7f);
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
                 "Moedas: " + GameManager.moedas, placar, corDaMoeda);

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

            Escrever(Faixa(meio + (linha * 1.5f), dica.fontSize * 2f),
                     texto, dica, corDeDestaque);
        }

        // Os controles no rodape: e a unica tela onde o jogador tem tempo de ler.
        Escrever(Faixa(Screen.height - (linha * 1.1f), dica.fontSize * 2f),
                 "A D mover   ·   ESPAÇO pular   ·   J atirar   ·   K granada   ·   P pausa",
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
        float meio = (Screen.height * 0.5f) - linha;

        float centroDoTitulo = meio + (linha * 0.5f);
        Escrever(Faixa(centroDoTitulo, grande.fontSize * 2f),
                 "VOCÊ MORREU", grande, new Color(1f, 0.25f, 0.2f));

        string textoDica = "aperte qualquer tecla";
        float centroDaDica = meio + (linha * 2f);
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
}
