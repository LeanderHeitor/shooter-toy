using UnityEngine;

// Chefe.cs
// O inimigo que fecha a horda 5, a ultima da partida. Depois dos segundos de
// resistencia, a torneira de rebeldes fecha e entra ele, sozinho, dentro do cerco.
// A horda so acaba quando ele cai, e derrubar ele vence o jogo.
//
// E o unico inimigo do jogo com barra de vida. O resto morre com um tiro, e isso
// continua sendo a regra: o chefe e a excecao que faz a regra ser notada. E ele que
// da destino ao dinheiro guardado - a metralhadora comprada na horda 4 derrete a
// barra, a pistola arranha - entao a ganancia tem uma prova no fim de cada ciclo.
//
// Um script so para os dois chefes: o que muda entre eles sao os numeros e a arte,
// e a pergunta que cada um faz ao jogador. So a Minigun aparece na partida; o
// Di-Cokka ficou pronto, mas saiu quando o jogo foi encurtado de 10 para 5 hordas.
//   Minigun: rajada reta na altura do peito. A resposta e PULAR.
//   Di-Cokka: obus em arco que cai onde o jogador estava. A resposta e CORRER.
public class Chefe : MonoBehaviour
{
    // O chefe vivo agora, para o HUD desenhar a barra. Nulo fora da luta.
    public static Chefe atual;

    public string nome = "Rebelde da Minigun";

    // ----- ANDAR -----
    public float moveSpeed = 1.3f;
    public float distanciaDeTiro = 7f;   // perto disto ele para e ataca
    public float meiaLarguraDoCorpo = 1f; // quanto ele ocupa para cada lado, contra a parede

    // ----- ATAQUE -----
    // A mira e o aviso. Sem ela a rajada chegaria sem o jogador ter como reagir, e
    // morrer sem ter visto o perigo e o oposto do que o jogo inteiro promete.
    public float tempoDeMira = 0.8f;
    public int disparosPorAtaque = 6;
    public float intervaloEntreDisparos = 0.13f;
    public float descansoEntreAtaques = 2f;

    // A rajada vem em grupos. O pulo do Tarma deixa os pes acima da bala por menos
    // de meio segundo: seis tiros colados seriam um muro impossivel de pular. Em
    // grupos de tres com um respiro no meio, a rajada vira um ritmo - pula, pousa,
    // pula - e e isso que a torna justa. 0 = sem grupos.
    public int disparosPorGrupo = 0;
    public float pausaEntreGrupos = 0.55f;

    // Abaixo da metade da vida o descanso encurta. E o jeito mais barato de a luta
    // ter um fim diferente do comeco: o jogador sente que o chefe ficou desesperado.
    public float descansoNaFuria = 1.1f;

    // De onde sai o tiro, em unidades de mundo, para a FRENTE e para cima dos pes.
    public Vector2 bocaDaArma = new Vector2(1.6f, 0.75f);

    // Bala reta (Minigun): o mesmo prefab da bala do fuzileiro.
    public GameObject balaPrefab;
    public float velocidadeDaBala = 9f;

    // Obus em arco (Di-Cokka). Com isto ligado o chefe nao usa o balaPrefab.
    public bool lancaObus = false;
    public float tempoDeVooDoObus = 1.1f;
    public float alturaDoObus = 4f;
    public float raioDoObus = 1.3f;
    public Sprite[] quadrosDoObus;

    // Encostar no chefe mata, como no rebelde da faca. Sem isto a resposta para
    // qualquer chefe seria ficar colado nele, onde o tiro nao alcanca.
    public bool mataNoEncostao = true;

    // ----- ARTE -----
    // Todas as folhas olham para a ESQUERDA, como as dos rebeldes.
    public Sprite[] quadrosAndando;
    public Sprite[] quadrosMirando;
    public Sprite[] quadrosAtirando;
    public Sprite[] quadrosDestruido;    // vazio = so pisca e some
    public Sprite[] quadrosDaExplosao;
    public float quadrosPorSegundo = 12f;

    enum Fase { Andando, Mirando, Atirando }

    private Fase fase = Fase.Andando;
    private float faseComecouEm;
    private float proximoAtaque;
    private int disparosFeitos;
    private float proximoDisparo;
    private float direcaoTravada = -1f;   // o lado da rajada nao muda no meio dela

    private EnemyDeath morte;
    private SpriteRenderer spriteRenderer;
    private Transform alvo;
    private PlayerScript alvoScript;

    private float morreuEm = -1f;
    private float proximaExplosao;

    void Awake()
    {
        atual = this;
        morte = GetComponent<EnemyDeath>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnDestroy()
    {
        if (atual == this) atual = null;
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            alvo = player.transform;
            alvoScript = player.GetComponent<PlayerScript>();
        }

        // Entra andando antes do primeiro ataque: o jogador precisa de um instante
        // para ler quem chegou e o tamanho da barra.
        proximoAtaque = Time.time + 1.5f;
        faseComecouEm = Time.time;
    }

    // Quanto da barra ainda resta, de 0 a 1.
    public float FracaoDaVida()
    {
        if (morte == null || morte.vidaMaxima <= 0) return 0f;
        return Mathf.Clamp01((float)morte.vida / morte.vidaMaxima);
    }

    public bool EstaMorto()
    {
        return morte == null || morte.isMorto;
    }

    void Update()
    {
        if (EstaMorto())
        {
            Morrendo();
            return;
        }

        if (alvo == null) return;
        if (alvoScript != null && alvoScript.isMorto) { Mostrar(quadrosAndando, 0f, false); return; }

        float lado = (alvo.position.x > transform.position.x) ? 1f : -1f;
        float distancia = Mathf.Abs(alvo.position.x - transform.position.x);

        switch (fase)
        {
            case Fase.Andando:
                Virar(lado);

                bool longe = distancia > distanciaDeTiro;
                if (longe) Andar(lado);

                Mostrar(quadrosAndando, faseComecouEm, longe == false);

                if (longe == false && Time.time >= proximoAtaque)
                {
                    // O lado e decidido aqui e nao muda ate a rajada acabar: dar a
                    // volta por tras do chefe durante a mira e uma saida valida.
                    direcaoTravada = lado;
                    TrocarPara(Fase.Mirando);
                }
                break;

            case Fase.Mirando:
                Mostrar(quadrosMirando, faseComecouEm, false, true);
                if (Time.time - faseComecouEm >= tempoDeMira)
                {
                    disparosFeitos = 0;
                    proximoDisparo = Time.time;
                    TrocarPara(Fase.Atirando);
                }
                break;

            case Fase.Atirando:
                Mostrar(quadrosAtirando, faseComecouEm, false);
                if (Time.time >= proximoDisparo)
                {
                    Disparar();
                    disparosFeitos = disparosFeitos + 1;

                    bool fechouGrupo = disparosPorGrupo > 0 && (disparosFeitos % disparosPorGrupo) == 0;
                    proximoDisparo = Time.time + (fechouGrupo ? pausaEntreGrupos : intervaloEntreDisparos);
                }

                if (disparosFeitos >= disparosPorAtaque)
                {
                    bool furioso = FracaoDaVida() <= 0.5f;
                    proximoAtaque = Time.time + (furioso ? descansoNaFuria : descansoEntreAtaques);
                    TrocarPara(Fase.Andando);
                }
                break;
        }
    }

    void TrocarPara(Fase nova)
    {
        fase = nova;
        faseComecouEm = Time.time;
    }

    void Andar(float lado)
    {
        Vector3 p = transform.position;
        p.x = p.x + (lado * moveSpeed * Time.deltaTime);

        // O tanque e largo: parar pelo centro deixaria meio tanque dentro da parede.
        if (Cerco.fechado)
            p.x = Mathf.Clamp(p.x, Cerco.limiteEsquerdo + meiaLarguraDoCorpo,
                                   Cerco.limiteDireito - meiaLarguraDoCorpo);

        transform.position = p;
    }

    void Virar(float lado)
    {
        // A folha olha para a esquerda, entao o espelho e ao contrario do Tarma.
        if (spriteRenderer != null) spriteRenderer.flipX = lado > 0f;
    }

    // "parado" mostra so o primeiro quadro: andar no lugar parece patinar.
    // "umaVez" segura o ultimo quadro em vez de voltar ao primeiro.
    void Mostrar(Sprite[] quadros, float desde, bool parado, bool umaVez = false)
    {
        if (spriteRenderer == null || quadros == null || quadros.Length == 0) return;

        int q = parado ? 0 : (int)((Time.time - desde) * quadrosPorSegundo);
        q = umaVez ? Mathf.Min(q, quadros.Length - 1) : q % quadros.Length;
        spriteRenderer.sprite = quadros[q];
    }

    void Disparar()
    {
        Vector3 boca = transform.position +
                       new Vector3(direcaoTravada * bocaDaArma.x, bocaDaArma.y, 0f);

        if (lancaObus)
        {
            // Mira onde o jogador esta AGORA, nao onde ele vai estar. E isso que faz
            // correr funcionar: quem fica parado leva, quem se move escapa.
            float alvoX = (alvo != null) ? Cerco.ManterDentro(alvo.position.x) : boca.x;
            Obus.Lancar(boca, alvoX, transform.position.y, tempoDeVooDoObus, alturaDoObus,
                        raioDoObus, quadrosDoObus, quadrosDaExplosao);
            return;
        }

        if (balaPrefab == null) return;

        GameObject bala = Instantiate(balaPrefab, boca, Quaternion.identity);
        BulletScript bs = bala.GetComponent<BulletScript>();
        if (bs != null)
        {
            bs.isBalaInimiga = true;
            bs.direction = direcaoTravada;
            bs.moveSpeed = velocidadeDaBala;
        }
    }

    // O encostao. Enter e Stay pelo mesmo motivo do rebelde da faca.
    void OnTriggerEnter2D(Collider2D outro) { Encostou(outro); }
    void OnTriggerStay2D(Collider2D outro)  { Encostou(outro); }

    void Encostou(Collider2D outro)
    {
        if (mataNoEncostao == false || EstaMorto()) return;

        PlayerScript player = outro.GetComponent<PlayerScript>();
        if (player != null) player.Morrer();
    }

    // A queda do chefe precisa ser maior que a de um rebelde: foi a luta mais longa
    // da partida, e o fim dela e a recompensa visual por ter aguentado. Explosoes
    // espalhadas pelo corpo, a camera tremendo, e a carcaca some devagar.
    void Morrendo()
    {
        if (morreuEm < 0f)
        {
            morreuEm = Time.time;
            proximaExplosao = Time.time;
            Granada.Tremer(0.5f);
        }

        float desde = Time.time - morreuEm;
        float duracao = (morte != null) ? morte.tempoAteSumir : 2f;

        if (quadrosDestruido != null && quadrosDestruido.Length > 0)
            Mostrar(quadrosDestruido, morreuEm, false, true);

        if (Time.time >= proximaExplosao && desde < duracao * 0.7f)
        {
            proximaExplosao = Time.time + 0.18f;

            Bounds b = (spriteRenderer != null) ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.one);
            Vector3 onde = new Vector3(Random.Range(b.min.x, b.max.x) * 0.8f + b.center.x * 0.2f,
                                       Random.Range(b.min.y, b.max.y), 0f);
            Quadros.Explodir(quadrosDaExplosao, onde, Random.Range(1f, 1.6f), 5);
        }

        // Some no ultimo terco, para nao sumir de um quadro para o outro.
        if (spriteRenderer != null)
        {
            float t = Mathf.InverseLerp(duracao * 0.66f, duracao, desde);
            Color c = spriteRenderer.color;
            c.a = 1f - t;
            spriteRenderer.color = c;
        }
    }
}
