using UnityEngine;
using UnityEngine.InputSystem; // sistema de input NOVO da Unity (padrao deste projeto)
using UnityEngine.SceneManagement;

// GameManager.cs
// Guarda o placar da partida e manda recarregar a cena quando o jogador pede.
// Os campos "static" NAO se perdem quando a cena recarrega, entao o Awake precisa
// zerar na mao tudo o que vale so pra uma partida. So o recorde atravessa.
public class GameManager : MonoBehaviour
{
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

    // ----- FIM DE JOGO -----
    public static bool isFimDeJogo = false;

    void Awake()
    {
        // Cada partida comeca do zero. O recorde continua de onde estava.
        abates = 0;
        sequencia = 0;
        fimDaSequencia = 0f;
        isFimDeJogo = false;
        janela = janelaDaSequencia;

        // Rede de seguranca: se a partida anterior acabou congelada, descongela.
        Time.timeScale = 1f;
    }

    void Update()
    {
        // A sequencia expira sozinha. Com o jogo congelado o Time.time nao anda,
        // entao ela fica parada na tela de fim de jogo, que e o que a gente quer.
        if (sequencia > 0 && Time.time >= fimDaSequencia)
        {
            sequencia = 0;
        }

        if (isFimDeJogo == true && ApertouAlgumaTecla() == true)
        {
            Reiniciar();
        }
    }

    // Qualquer tecla ou o botao do mouse serve pra jogar de novo.
    // Uso "wasPressedThisFrame": segurar uma tecla desde antes de morrer nao reinicia sozinho.
    bool ApertouAlgumaTecla()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.anyKey.wasPressedThisFrame) return true;

        Mouse m = Mouse.current;
        if (m != null && m.leftButton.wasPressedThisFrame) return true;

        return false;
    }

    public static void ContarAbate()
    {
        abates = abates + 1;
        if (abates > recorde) recorde = abates;

        sequencia = sequencia + 1;
        fimDaSequencia = Time.time + janela;
    }

    // Chamado pelo PlayerScript depois que a animacao de morte termina.
    public static void FimDeJogo()
    {
        if (isFimDeJogo == true) return;

        isFimDeJogo = true;

        // Congela o mundo. O Update continua rodando, entao a tecla ainda e lida.
        Time.timeScale = 0f;
    }

    public static void Reiniciar()
    {
        // Tem que destravar ANTES de carregar, senao a partida seguinte nasce congelada.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
