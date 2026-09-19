using UnityEngine;
using UnityEngine.InputSystem; // sistema de input NOVO da Unity (padrao deste projeto)
using UnityEngine.SceneManagement;

// Os estados em que o jogo pode estar.
// Uma "maquina de estados" e so isto: uma variavel que diz em qual tela o jogo esta,
// e um lugar unico que decide como sair de uma tela para outra. O jogo inteiro roda
// numa cena so - o menu nao e outra cena, e o mesmo mundo com o tempo parado e um
// texto por cima. E por isso que trocar de tela aqui e instantaneo.
//
// Jogando e o UNICO estado em que o tempo anda. Os outros tres congelam o mundo.
public enum Estado
{
    Menu,       // antes de comecar. O mundo ja existe atras do texto, so nao anda
    Jogando,    // a partida em si
    Pausado,    // congelado a pedido do jogador, com a partida intacta
    FimDeJogo   // congelado porque o jogador morreu
}

// GameManager.cs
// Guarda o placar da partida, o estado do jogo, e manda recarregar a cena.
// Os campos "static" NAO se perdem quando a cena recarrega, entao o Awake precisa
// zerar na mao tudo o que vale so pra uma partida. So o recorde atravessa.
public class GameManager : MonoBehaviour
{
    // ----- ESTADO -----
    public static Estado estado = Estado.Menu;

    // ----- PLACAR -----
    public static int abates = 0;
    public static int recorde = 0;

    // ----- SEQUENCIA DE ABATES -----
    // Conta abates encadeados. Se passar "janelaDaSequencia" sem matar ninguem, zera.
    // E o unico mecanismo do jogo que pune ficar recuando: parado, a sequencia morre.
    public static int sequencia = 0;
    public float janelaDaSequencia = 3f;
    private static float janela = 3f;   // copia static, porque ContarAbate e static
    private static float fimDaSequencia = 0f;

    void Awake()
    {
        // Cada partida comeca do zero. O recorde continua de onde estava.
        abates = 0;
        sequencia = 0;
        fimDaSequencia = 0f;
        janela = janelaDaSequencia;

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
        switch (estado)
        {
            case Estado.Menu:
                if (ApertouAlgumaTecla()) IrPara(Estado.Jogando);
                break;

            case Estado.Jogando:
                if (ApertouPausa()) IrPara(Estado.Pausado);
                break;

            case Estado.Pausado:
                if (ApertouPausa()) IrPara(Estado.Jogando);
                break;

            case Estado.FimDeJogo:
                // Recarrega a cena em vez de so trocar de estado: inimigos, tiros e
                // a posicao do jogador precisam voltar ao inicio, e recarregar e
                // mais confiavel do que tentar desfazer cada coisa na mao.
                if (ApertouAlgumaTecla()) Reiniciar();
                break;
        }
    }

    // O UNICO lugar do projeto que mexe no timeScale. Concentrar aqui evita o bug
    // classico de uma tela congelar o jogo e esquecer de descongelar na saida.
    public static void IrPara(Estado novo)
    {
        estado = novo;
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

    // Esc E P pausam. O Esc e o que todo mundo tenta primeiro, mas no NAVEGADOR ele
    // e do browser antes de ser do jogo (e a tecla que sai da tela cheia), entao o P
    // existe como saida garantida no build WebGL.
    static bool ApertouPausa()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        return kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame;
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
        if (estado == Estado.FimDeJogo) return;
        IrPara(Estado.FimDeJogo);
    }

    public static void Reiniciar()
    {
        // Destrava antes de carregar. O Awake da cena nova congela de novo no menu,
        // mas deixar o timeScale em 0 durante a troca nao tem vantagem nenhuma.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
