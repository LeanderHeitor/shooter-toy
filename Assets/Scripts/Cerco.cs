using UnityEngine;

// Cerco.cs
// As duas paredes que prendem o jogador no mesmo pedaco de mundo enquanto uma horda
// esta viva.
//
// Existe por um motivo de experiencia, nao de tecnica: sem ele o jogador anda para
// tras para sempre, atira de longe e nenhuma horda tem tensao. Com ele, o tiroteio
// tem um tamanho, e a moeda caida no meio dele vira uma decisao em vez de um troco.
//
// As paredes sao construidas em codigo, nao arrastadas no Inspector. Sao dois
// retangulos coloridos: fabricar isso na mao custaria dois prefabs, dois sprites e
// duas referencias para arrastar, e qualquer uma delas esquecida quebraria a horda
// inteira sem dizer por que.
public class Cerco : MonoBehaviour
{
    // Alta de proposito: o jogador pula, e uma parede na altura dele seria pulavel.
    public float altura = 30f;
    public float espessura = 0.8f;

    // Onde apoiar a parede. NAO e ajustavel aqui: a Horda copia este valor do
    // Spawner no Start, porque o chao ja tem um dono e dois campos que precisam ser
    // iguais na mao acabam diferentes sem ninguem perceber.
    [HideInInspector] public float alturaDoChao = -3.75f;

    // Translucida: ela precisa ser lida como limite sem esconder o inimigo que
    // estiver encostado nela.
    public Color corNormal = new Color(1f, 0.35f, 0.28f, 0.30f);

    // Na Horda de Resistencia o cerco aperta, e a cor mais forte avisa que a regra
    // mudou antes de o jogador ler qualquer texto.
    public Color corDaResistencia = new Color(1f, 0.20f, 0.15f, 0.45f);

    // Onde as paredes estao agora. Static porque o Spawner precisa consultar isto a
    // cada inimigo que nasce, e ele nao tem motivo nenhum para guardar uma
    // referencia ao cerco so para fazer uma conta.
    public static bool fechado = false;
    public static float limiteEsquerdo = 0f;
    public static float limiteDireito = 0f;

    // Ninguem nasce colado na parede: sem esta folga um inimigo empurrado para
    // dentro ficaria metade dentro e metade fora dela.
    private const float folgaDaParede = 1f;

    private Transform paredeEsquerda;
    private Transform paredeDireita;
    private SpriteRenderer corEsquerda;
    private SpriteRenderer corDireita;

    // Um retangulo branco de 1x1 unidade, tingido pelo SpriteRenderer de cada parede.
    // Um so para as duas, e para todas as hordas da sessao.
    private static Sprite spriteDaParede;

    void Awake()
    {
        // Os campos static sobrevivem ao recarregamento da cena; o cerco, nao.
        // Sem esta linha a primeira horda da partida seguinte nasceria achando que
        // ja estava cercada.
        fechado = false;

        paredeEsquerda = CriarParede("Parede esquerda");
        paredeDireita = CriarParede("Parede direita");

        corEsquerda = paredeEsquerda.GetComponent<SpriteRenderer>();
        corDireita = paredeDireita.GetComponent<SpriteRenderer>();
    }

    // Chamado pela Horda quando uma horda comeca. O centro e a posicao do jogador
    // naquele instante: o cerco nasce ao redor de onde ele esta, e nao num lugar
    // fixo do mundo, porque o mundo deste jogo nao tem lugar fixo.
    public void Fechar(float centroX, float meiaLargura, bool ehResistencia)
    {
        limiteEsquerdo = centroX - meiaLargura;
        limiteDireito = centroX + meiaLargura;
        fechado = true;

        Color cor = ehResistencia ? corDaResistencia : corNormal;

        Posicionar(paredeEsquerda, corEsquerda, limiteEsquerdo, cor);
        Posicionar(paredeDireita, corDireita, limiteDireito, cor);
    }

    public void Abrir()
    {
        fechado = false;
        paredeEsquerda.gameObject.SetActive(false);
        paredeDireita.gameObject.SetActive(false);
    }

    // Puxa uma posicao para dentro do cerco. Devolve o x sem mexer se o cerco estiver
    // aberto, entao quem chama nao precisa perguntar antes se existe cerco.
    public static float ManterDentro(float x)
    {
        if (fechado == false) return x;
        return Mathf.Clamp(x, limiteEsquerdo + folgaDaParede, limiteDireito - folgaDaParede);
    }

    void Posicionar(Transform parede, SpriteRenderer cordaParede, float x, Color cor)
    {
        // A parede sobe a partir do chao, e ainda desce um pouco abaixo dele para
        // nao abrir fresta se o piso oscilar.
        float centroY = alturaDoChao + (altura * 0.5f) - 2f;

        parede.position = new Vector3(x, centroY, 0f);
        parede.localScale = new Vector3(espessura, altura, 1f);
        cordaParede.color = cor;

        parede.gameObject.SetActive(true);
    }

    Transform CriarParede(string nome)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(transform, false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteDaParede();
        // A cor nao e definida aqui: quem pinta e o Fechar, que e quem sabe se a
        // horda e comum ou de resistencia. A parede nasce desligada de qualquer jeito.
        // Na frente do cenario e do chao, atras de ninguem: a parede nunca deve
        // cobrir o personagem que estiver colado nela.
        sr.sortingOrder = -1;

        // O colisor mede 1x1 e cresce junto com a escala do objeto, entao a espessura
        // e a altura tem um lugar so para serem ajustadas.
        BoxCollider2D caixa = go.AddComponent<BoxCollider2D>();
        caixa.size = Vector2.one;

        go.SetActive(false);
        return go.transform;
    }

    static Sprite SpriteDaParede()
    {
        if (spriteDaParede != null) return spriteDaParede;

        // Texture2D.whiteTexture ja vem com a Unity e existe em qualquer plataforma,
        // inclusive no WebGL. Com pixelsPerUnit igual a largura dela, o sprite sai
        // medindo exatamente 1 unidade, que e o que deixa a escala do transform ser
        // lida direto em unidades de mundo.
        Texture2D branco = Texture2D.whiteTexture;
        spriteDaParede = Sprite.Create(branco,
                                       new Rect(0f, 0f, branco.width, branco.height),
                                       new Vector2(0.5f, 0.5f),
                                       branco.width);
        return spriteDaParede;
    }
}
