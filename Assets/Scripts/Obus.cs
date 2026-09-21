using UnityEngine;

// Obus.cs
// O tiro do tanque: sobe em arco e cai num ponto do chao, explodindo ali.
//
// Nao e uma bala. A bala reta se responde pulando, e isso o jogador ja aprendeu com
// o fuzileiro e com a Minigun. O obus pede outra coisa - sair de onde esta - e a
// marca vermelha no chao existe para que essa decisao seja possivel: ela aparece no
// instante do disparo, e o jogador tem o tempo do voo para sair de cima dela.
//
// Montado em codigo, sem prefab, como as paredes do Cerco.
public class Obus : MonoBehaviour
{
    private Vector3 origem;
    private float alvoX;
    private float chaoY;
    private float duracao;
    private float altura;
    private float raio;
    private float lancadoEm;
    private Sprite[] quadrosDaExplosao;

    private Transform marca;
    private SpriteRenderer corDaMarca;

    private static Sprite spriteDaMarca;

    public static void Lancar(Vector3 de, float alvoX, float chaoY, float duracao, float altura,
                              float raio, Sprite[] quadrosDoVoo, Sprite[] quadrosDaExplosao)
    {
        GameObject go = new GameObject("Obus");
        go.transform.position = de;
        go.transform.localScale = Vector3.one * 1.3f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 4;

        Quadros q = go.AddComponent<Quadros>();
        q.quadros = quadrosDoVoo;
        q.quadrosPorSegundo = 16f;

        Obus o = go.AddComponent<Obus>();
        o.origem = de;
        o.alvoX = alvoX;
        o.chaoY = chaoY;
        o.duracao = Mathf.Max(0.2f, duracao);
        o.altura = altura;
        o.raio = raio;
        o.quadrosDaExplosao = quadrosDaExplosao;
    }

    void Start()
    {
        lancadoEm = Time.time;

        // A marca e filha de ninguem: o obus gira no ar, e ela tem que ficar deitada.
        GameObject m = new GameObject("Marca do obus");
        marca = m.transform;
        marca.position = new Vector3(alvoX, chaoY + 0.05f, 0f);
        marca.localScale = new Vector3(raio * 2f, 0.18f, 1f);
        corDaMarca = m.AddComponent<SpriteRenderer>();
        corDaMarca.sprite = SpriteDaMarca();
        corDaMarca.sortingOrder = 2;
    }

    void Update()
    {
        float t = (Time.time - lancadoEm) / duracao;

        if (t >= 1f)
        {
            Explodir();
            return;
        }

        // Parabola simples: anda reto de um ponto ao outro e soma uma curva que e zero
        // nas pontas e "altura" no meio. Sem Rigidbody: o obus tem que cair EXATAMENTE
        // na marca, e a fisica acertaria so mais ou menos.
        Vector3 anterior = transform.position;
        float x = Mathf.Lerp(origem.x, alvoX, t);
        float y = Mathf.Lerp(origem.y, chaoY, t) + (altura * 4f * t * (1f - t));
        transform.position = new Vector3(x, y, 0f);

        // A chama da folha aponta para a esquerda; gira para seguir a direcao do voo.
        Vector3 v = transform.position - anterior;
        if (v.sqrMagnitude > 0.0001f)
        {
            float angulo = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo + 180f);
        }

        // A marca pisca cada vez mais rapido conforme o obus chega.
        if (corDaMarca != null)
        {
            float pisca = 0.35f + 0.25f * Mathf.Sin(Time.time * Mathf.Lerp(8f, 30f, t));
            corDaMarca.color = new Color(1f, 0.2f, 0.15f, pisca);
        }
    }

    void Explodir()
    {
        Vector3 ponto = new Vector3(alvoX, chaoY, 0f);
        Quadros.Explodir(quadrosDaExplosao, ponto + new Vector3(0f, 0.6f, 0f), 1.4f, 5);
        Granada.Tremer(0.15f);

        // Mesma regra do rocket: conta so a distancia no chao. E um pulo alto por
        // cima da explosao tambem escapa, porque o fogo nao sobe tanto.
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            bool perto = Mathf.Abs(go.transform.position.x - alvoX) <= raio;
            bool noAlto = go.transform.position.y - chaoY > 2f;
            PlayerScript p = go.GetComponent<PlayerScript>();
            if (perto && noAlto == false && p != null) p.Morrer();
        }

        Destroy(gameObject);
    }

    // A granada leva os obuses em voo junto, e a marca vai com eles.
    void OnDestroy()
    {
        if (marca != null) Destroy(marca.gameObject);
    }

    // Um quadrado branco de 1x1 unidade, tingido de vermelho pela marca.
    static Sprite SpriteDaMarca()
    {
        if (spriteDaMarca != null) return spriteDaMarca;

        Texture2D tex = Texture2D.whiteTexture;
        spriteDaMarca = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                      new Vector2(0.5f, 0.5f), tex.width);
        return spriteDaMarca;
    }
}
