using UnityEngine;

// Sangue.cs
// O espirro de sangue que sai do rebelde no instante do abate.
//
// O flash branco do EnemyDeath diz "acertou". O sangue diz de ONDE veio o tiro: ele
// espirra sempre para o lado oposto ao jogador, e com tres inimigos caindo ao mesmo
// tempo e isso que deixa o olho ligar cada morte a quem atirou.
//
// Toca uma vez e se apaga sozinho. Nao tem colisor nem regra de jogo: e so imagem.
public class Sangue : MonoBehaviour
{
    // Os quadros do espirro (Art/Characters/Blood_Spray). O desenho original espirra
    // para a DIREITA; o lado esquerdo e o mesmo desenho espelhado.
    public Sprite[] quadros;
    public float quadrosPorSegundo = 20f;

    // Mesmo motivo da Moeda: quem mata nao precisa saber onde mora o arquivo.
    private static GameObject prefab;

    private float nasceuEm;
    private SpriteRenderer spriteRenderer;

    // "paraDireita" e o lado para onde o jato vai, nao o lado de quem atirou.
    public static void Soltar(Vector3 onde, bool paraDireita)
    {
        if (prefab == null) prefab = Resources.Load<GameObject>("Sangue");
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, onde, Quaternion.identity);

        // Espelha pela escala e nao pelo flipX: o pivo do desenho fica na origem do
        // jato, e a escala espelha em volta do pivo. O flipX espelharia o desenho
        // dentro do proprio retangulo, e o jato nasceria longe do corpo.
        if (paraDireita == false) go.transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    void Start()
    {
        nasceuEm = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (quadros == null || quadros.Length == 0 || spriteRenderer == null)
        {
            Destroy(gameObject);
            return;
        }

        int quadro = (int)((Time.time - nasceuEm) * quadrosPorSegundo);
        if (quadro >= quadros.Length)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = quadros[quadro];
    }
}
