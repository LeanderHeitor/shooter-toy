using UnityEngine;

// Explosao.cs
// O clarao redondo do rocket: um circulo laranja que cresce e some em um quarto de
// segundo.
//
// Nao e enfeite. O rocket mata quem estiver PERTO do acerto, e nao so quem ele
// tocou; sem o circulo o jogador ve tres rebeldes caindo de uma bala so e nao
// entende por que. O tamanho do circulo e o tamanho do raio, entao ele mostra
// exatamente ate onde a explosao chegou.
//
// Montada inteira em codigo, como as paredes do Cerco: um circulo tingido nao
// justifica um prefab e um sprite no projeto.
public class Explosao : MonoBehaviour
{
    public float duracao = 0.25f;

    private SpriteRenderer desenho;
    private float raio;
    private float idade = 0f;

    // Um circulo branco de 1 unidade de diametro, gerado uma vez por sessao e
    // tingido pelo SpriteRenderer de cada explosao.
    private static Sprite circulo;

    public static void Criar(Vector3 onde, float raio)
    {
        GameObject go = new GameObject("Explosao");
        go.transform.position = onde;

        Explosao e = go.AddComponent<Explosao>();
        e.raio = raio;
        e.desenho = go.AddComponent<SpriteRenderer>();
        e.desenho.sprite = Circulo();
        e.desenho.sortingOrder = 5;   // por cima dos rebeldes que ela acabou de matar
        e.Atualizar();

        Granada.Tremer(0.25f);
    }

    void Update()
    {
        idade = idade + Time.deltaTime;
        if (idade >= duracao)
        {
            Destroy(gameObject);
            return;
        }
        Atualizar();
    }

    void Atualizar()
    {
        float t = Mathf.Clamp01(idade / duracao);

        // Nasce ja com metade do tamanho: comecar do zero faria o primeiro quadro,
        // que e o que o olho mais registra, parecer uma faisca.
        float diametro = raio * 2f * Mathf.Lerp(0.5f, 1f, t);
        transform.localScale = new Vector3(diametro, diametro, 1f);

        desenho.color = new Color(1f, Mathf.Lerp(0.85f, 0.35f, t), 0.15f, 0.8f * (1f - t));
    }

    static Sprite Circulo()
    {
        if (circulo != null) return circulo;

        const int lado = 64;
        Texture2D tex = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
        float centro = (lado - 1) * 0.5f;

        for (int y = 0; y < lado; y++)
        {
            for (int x = 0; x < lado; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro)) / centro;
                // Borda macia no ultimo decimo, senao o circulo sai serrilhado.
                float a = Mathf.Clamp01((1f - d) * 10f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();

        // "lado" pixels por unidade: o sprite mede exatamente 1 unidade, e a escala
        // do transform vira o diametro sem conta nenhuma.
        circulo = Sprite.Create(tex, new Rect(0, 0, lado, lado), new Vector2(0.5f, 0.5f), lado);
        return circulo;
    }
}
