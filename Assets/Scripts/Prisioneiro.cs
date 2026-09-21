using UnityEngine;

// Prisioneiro.cs
// Quem atende a loja. Aparece amarrado quando o cerco abre, e se solta quando o
// jogador chega perto - dai em diante ele fica de pe esperando, e apertar E ao lado
// dele abre a loja.
//
// Nao anda, nao morre e nao encosta em ninguem: e o unico personagem do jogo que
// nao quer matar o jogador, e por isso nao tem colisor nem fisica. "Estar perto" e
// uma conta de distancia, feita pela Loja.
//
// Mora num prefab em Assets/Resources, pelo mesmo motivo da Moeda: quem precisa
// dele e a Loja, que e montada em codigo e nao tem onde receber um arrasto.
public class Prisioneiro : MonoBehaviour
{
    // Os quadros das duas folhas (Art/Characters/POW_Tied e POW_Idle). Um vetor e
    // uma divisao fazem o que um Animator com dois clipes faria, como na Moeda.
    public Sprite[] quadrosAmarrado;
    public Sprite[] quadrosSolto;
    public float quadrosPorSegundo = 8f;

    // Ate onde o jogador pode estar para conversar com ele.
    public float distanciaDeConversa = 1.6f;

    // Solto na primeira vez que o jogador chega perto. Nao volta a ser amarrado: ele
    // e o premio de ter limpado a horda, e o jogador precisa ver que o libertou.
    public bool solto = false;

    private SpriteRenderer spriteRenderer;
    private float nasceuEm;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        nasceuEm = Time.time;
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        Sprite[] quadros = solto ? quadrosSolto : quadrosAmarrado;
        if (quadros == null || quadros.Length == 0) return;

        int quadro = (int)((Time.time - nasceuEm) * quadrosPorSegundo) % quadros.Length;
        spriteRenderer.sprite = quadros[quadro];
    }

    // So a distancia no chao: o jogador pulando em cima dele continua perto.
    public bool Perto(float xDoJogador)
    {
        return Mathf.Abs(xDoJogador - transform.position.x) <= distanciaDeConversa;
    }
}
