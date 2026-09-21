using UnityEngine;

// Quadros.cs
// Troca o sprite por uma lista de quadros, em loop ou uma vez so.
//
// E o mesmo truque da Moeda: para uma animacao sem estados nem transicoes, um
// vetor e uma divisao fazem o que um Animator com controller e clip faria em tres
// arquivos. Serve para a bala do tanque, que so gira a chama, e para as explosoes
// dos chefes, que tocam uma vez e somem.
public class Quadros : MonoBehaviour
{
    public Sprite[] quadros;
    public float quadrosPorSegundo = 14f;

    // false = toca uma vez e destroi o objeto no ultimo quadro.
    public bool emLoop = true;

    private SpriteRenderer spriteRenderer;
    private float comecouEm;

    // Uma explosao de fumaca num ponto. Nao precisa de prefab: e um sprite e este
    // componente, e montar isso em codigo custa menos que um arquivo a mais.
    public static void Explodir(Sprite[] quadros, Vector3 onde, float escala, int ordem)
    {
        if (quadros == null || quadros.Length == 0) return;

        GameObject go = new GameObject("Explosao do chefe");
        go.transform.position = onde;
        go.transform.localScale = Vector3.one * escala;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = ordem;

        Quadros q = go.AddComponent<Quadros>();
        q.quadros = quadros;
        q.emLoop = false;
        q.quadrosPorSegundo = 18f;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        comecouEm = Time.time;
        if (spriteRenderer != null && quadros != null && quadros.Length > 0)
            spriteRenderer.sprite = quadros[0];
    }

    void Update()
    {
        if (spriteRenderer == null || quadros == null || quadros.Length == 0) return;

        int quadro = (int)((Time.time - comecouEm) * quadrosPorSegundo);

        if (emLoop == false && quadro >= quadros.Length)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = quadros[quadro % quadros.Length];
    }
}
