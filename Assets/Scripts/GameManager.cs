using UnityEngine;
using UnityEngine.InputSystem; // sistema de input NOVO da Unity (padrao deste projeto)
using UnityEngine.SceneManagement;

// Os estados em que o jogo pode estar.
// Uma "maquina de estados" e so isto: uma variavel que diz em qual tela o jogo esta,
// e um lugar unico que decide como sair de uma tela para outra. O jogo inteiro roda
// numa cena so - o menu nao e outra cena, e o mesmo mundo com o tempo parado e um
// texto por cima. E por isso que trocar de tela aqui e instantaneo.
//
// Jogando e o UNICO estado em que o tempo anda. Os outros congelam o mundo.
public enum Estado
{
    Menu,       // antes de comecar. O mundo ja existe atras do texto, so nao anda
    Jogando,    // a partida em si
    Pausado,    // congelado a pedido do jogador, com a partida intacta
    Loja,       // congelado enquanto o jogador compra do prisioneiro, entre hordas
    FimDeJogo,  // congelado porque o jogador morreu
    Vitoria     // congelado porque o chefe da ultima horda caiu
}

// GameManager.cs
// Guarda o placar da partida, o estado do jogo, e manda recarregar a cena.
// Os campos "static" NAO se perdem quando a cena recarrega, entao o Awake precisa
// zerar na mao tudo o que vale so pra uma partida. So o recorde atravessa.
public class GameManager : MonoBehaviour
{
    // ----- ESTADO -----
    public static Estado estado = Estado.Menu;

    // Em que quadro o estado mudou pela ultima vez. Duas telas escutam o Esc (a
    // pausa aqui, a saida da loja na Loja), e a ordem em que a Unity chama os Update
    // nao e garantida: sem esta trava, o Esc que fecha a loja podia ser lido de novo
    // no mesmo quadro como "pausar", e o jogador saia da loja direto para a pausa.
    public static int quadroDaUltimaTroca = -1;

    // ----- PLACAR -----
    public static int abates = 0;
    public static int recorde = 0;

    // A horda mais longe alcancada na sessao. Anda junto com o recorde de abates,
    // mas conta outra coisa: da para morrer com muitos abates numa horda baixa
    // (matando devagar) ou com poucos numa horda alta (correndo atras da moeda).
    public static int recordeDeHorda = 0;

    // O recorde como estava ANTES desta partida. O recorde de verdade sobe no
    // instante do abate, entao no fim de jogo ele ja e igual aos abates e nao da
    // para dizer "faltaram 7". Guardar o de antes e o que permite a comparacao.
    public static int recordeAntesDaPartida = 0;

    // A vitoria mais rica da sessao: quantas moedas sobraram na mao ao vencer. So
    // conta vitoria de proposito. Se contasse qualquer partida, o melhor jeito de
    // bater o recorde seria nao gastar nada e morrer rico, e isso mataria a loja e a
    // granada. Assim toda compra vira a pergunta "isto me ajuda a vencer, ou so me
    // faz vencer mais pobre?".
    public static int recordeDeMoedas = 0;
    public static bool bateuRecordeDeMoedas = false;

    // As telas finais ignoram teclas por um instante. Quem esta segurando ou
    // martelando o J no tiroteio pularia a tela de morte ou de vitoria sem nem ler.
    public float segundosAntesDeSair = 1.2f;
    private static float esperaAntesDeSair = 1.2f;
    private static float podeSairEm = 0f;

    // ----- MOEDAS -----
    // O unico recurso do jogo. NAO atravessa a morte: toda partida comeca pobre,
    // e e isso que torna cada gasto uma escolha em vez de um detalhe.
    public static int moedas = 0;

    // ----- SEQUENCIA DE ABATES -----
    // Conta abates encadeados. Se passar "janelaDaSequencia" sem matar ninguem, zera.
    // E o unico mecanismo do jogo que pune ficar recuando: parado, a sequencia morre.
    public static int sequencia = 0;
    public float janelaDaSequencia = 3f;
    private static float janela = 3f;   // copia static, porque ContarAbate e static
    private static float fimDaSequencia = 0f;

    // A cada quantos abates encadeados a sequencia paga uma moeda a mais por abate.
    public int abatesPorBonus = 5;
    private static int passoDoBonus = 5;

    void Awake()
    {
        // Cada partida comeca do zero. O recorde continua de onde estava.
        abates = 0;
        moedas = 0;
        recordeAntesDaPartida = recorde;
        bateuRecordeDeMoedas = false;
        esperaAntesDeSair = segundosAntesDeSair;
        sequencia = 0;
        fimDaSequencia = 0f;
        janela = janelaDaSequencia;
        passoDoBonus = abatesPorBonus;

        // A morte leva tudo, nao so as moedas: arma, municao e coletes tambem.
        Arsenal.Zerar();

        // A loja e montada em codigo pelo mesmo motivo do Cerco na Horda: um
        // componente a menos para esquecer de arrastar na cena.
        if (GetComponent<Loja>() == null) gameObject.AddComponent<Loja>();

        // A cena nasce no menu, congelada. Isso tambem serve de rede de seguranca:
        // se a partida anterior acabou com o tempo parado, o IrPara conserta.
        IrPara(Estado.Menu);
    }

    void Update()
    {
        // A sequencia expira sozinha. Com o jogo congelado o Time.time nao anda,
        // entao ela fica parada nas telas congeladas, que e o que a gente quer.
        if (sequencia > 0 && Time.time >= fimDaSequencia)
        {
            sequencia = 0;
        }

        // Cada estado escuta so as teclas que fazem sentido nele. Um switch em vez
        // de varios "if" soltos: assim e impossivel duas telas responderem a mesma
        // tecla ao mesmo tempo, que era o risco do bool isFimDeJogo antigo.
        // Uma tecla, uma troca de tela. Ver quadroDaUltimaTroca.
        if (Time.frameCount == quadroDaUltimaTroca) return;

        switch (estado)
        {
            case Estado.Menu:
                // O T comeca no modo de teste, ja adiantado e equipado. Vem antes
                // do "qualquer tecla", senao o T tambem contaria como comecar normal.
                if (ApertouTeste())
                {
                    Horda horda = FindFirstObjectByType<Horda>();
                    if (horda != null) horda.ComecarNoTeste();
                    IrPara(Estado.Jogando);
                }
                else if (ApertouAlgumaTecla()) IrPara(Estado.Jogando);
                break;

            case Estado.Jogando:
                if (ApertouPausa()) IrPara(Estado.Pausado);
                break;

            case Estado.Pausado:
                if (ApertouPausa()) IrPara(Estado.Jogando);
                break;

            case Estado.Loja:
                // Quem entra e sai da loja e a propria Loja: e ela que sabe se o
                // jogador esta perto do prisioneiro, e ela que le as teclas de compra.
                break;

            case Estado.FimDeJogo:
                // Recarrega a cena em vez de so trocar de estado: inimigos, tiros e
                // a posicao do jogador precisam voltar ao inicio, e recarregar e
                // mais confiavel do que tentar desfazer cada coisa na mao.
                if (Time.unscaledTime >= podeSairEm && ApertouAlgumaTecla()) Reiniciar();
                break;

            case Estado.Vitoria:
                // Igual ao fim de jogo: a partida acabou, a proxima nasce no menu.
                if (Time.unscaledTime >= podeSairEm && ApertouAlgumaTecla()) Reiniciar();
                break;
        }
    }

    // O UNICO lugar do projeto que mexe no timeScale. Concentrar aqui evita o bug
    // classico de uma tela congelar o jogo e esquecer de descongelar na saida.
    public static void IrPara(Estado novo)
    {
        estado = novo;
        quadroDaUltimaTroca = Time.frameCount;
        Time.timeScale = (novo == Estado.Jogando) ? 1f : 0f;
    }

    // Qualquer tecla ou o botao do mouse serve.
    // Uso "wasPressedThisFrame": segurar uma tecla desde antes nao dispara sozinho.
    static bool ApertouAlgumaTecla()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.anyKey.wasPressedThisFrame) return true;

        Mouse m = Mouse.current;
        if (m != null && m.leftButton.wasPressedThisFrame) return true;

        return false;
    }

    static bool ApertouTeste()
    {
        Keyboard kb = Keyboard.current;
        return kb != null && kb.tKey.wasPressedThisFrame;
    }

    // Esc E P pausam. O Esc e o que todo mundo tenta primeiro, mas no NAVEGADOR ele
    // e do browser antes de ser do jogo (e a tecla que sai da tela cheia), entao o P
    // existe como saida garantida no build WebGL.
    static bool ApertouPausa()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        return kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame;
    }

    // Chamado pela Moeda quando o jogador encosta nela, e pelo fim de horda.
    public static void GanharMoedas(int quanto)
    {
        if (quanto <= 0) return;
        moedas = moedas + quanto;
    }

    // Devolve false e nao cobra nada se o jogador nao tiver o suficiente. Quem
    // gasta pergunta ANTES de aplicar o efeito, senao dava para usar a granada
    // fiado. Um unico lugar cobrando evita saldo negativo espalhado pelo codigo.
    public static bool Gastar(int quanto)
    {
        if (quanto <= 0) return true;
        if (moedas < quanto) return false;

        moedas = moedas - quanto;
        return true;
    }

    // Chamado pela Horda quando uma horda e limpa. Conta a horda VENCIDA, nao a que
    // estava em andamento na hora da morte: morrer no meio da horda 7 nao e ter
    // chegado a horda 7.
    public static void RegistrarHorda(int numero)
    {
        if (numero > recordeDeHorda) recordeDeHorda = numero;
    }

    // Quantas moedas A MAIS cada abate paga por causa da sequencia atual.
    // E o que remunera o jogador por continuar avancando: a sequencia morre sozinha
    // se ele parar ou recuar, entao o bonus premia exatamente o comportamento que o
    // jogo quer, sem precisar avisar ninguem disso.
    public static int BonusDeSequencia()
    {
        if (passoDoBonus <= 0) return 0;
        return sequencia / passoDoBonus;
    }

    public static void ContarAbate()
    {
        abates = abates + 1;
        if (abates > recorde) recorde = abates;

        sequencia = sequencia + 1;
        fimDaSequencia = Time.time + janela;
    }

    // Chamado pelo PlayerScript depois que a animacao de morte termina.
    // Nome de EVENTO ("morreu"), nao de estado: o estado chama-se Estado.FimDeJogo,
    // e ter os dois com o mesmo nome deixava o codigo ambiguo de ler.
    public static void MorreuOJogador()
    {
        if (estado == Estado.FimDeJogo || estado == Estado.Vitoria) return;
        podeSairEm = Time.unscaledTime + esperaAntesDeSair;
        IrPara(Estado.FimDeJogo);
    }

    // Chamado pela Horda quando o chefe da ultima horda cai. As moedas da mao viram
    // placar: e aqui que o recorde de moedas e decidido.
    public static void Venceu()
    {
        if (estado == Estado.FimDeJogo || estado == Estado.Vitoria) return;

        if (moedas > recordeDeMoedas)
        {
            recordeDeMoedas = moedas;
            bateuRecordeDeMoedas = true;
        }

        podeSairEm = Time.unscaledTime + esperaAntesDeSair;
        IrPara(Estado.Vitoria);
    }

    public static void Reiniciar()
    {
        // Destrava antes de carregar. O Awake da cena nova congela de novo no menu,
        // mas deixar o timeScale em 0 durante a troca nao tem vantagem nenhuma.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
