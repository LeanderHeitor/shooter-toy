using UnityEngine;
using UnityEngine.InputSystem; // sistema de input NOVO da Unity (padrao deste projeto)
using Unity.Cinemachine;

// Granada.cs
// A acao especial do jogador: tecla K, mata de uma vez todo inimigo dentro do cerco.
//
// A vantagem e obvia - e o unico jeito do jogo de sair de um aperto sem mirar. A
// punicao e o preco: ela cobra moedas NA HORA do uso, e nao na loja. E isso que poe
// a decisao de ganancia dentro do tiroteio, onde ela doi: cada granada jogada agora
// e uma arma que o jogador nao compra no proximo intervalo.
//
// Vai no mesmo objeto do GameManager.
public class Granada : MonoBehaviour
{
    public int custo = 10;

    // Quanto a camera treme. O tremor nao e enfeite: e o que faz o jogador sentir
    // que gastou alguma coisa grande, e nao que apertou um botao de limpar a tela.
    public float forcaDoTremor = 0.6f;
    public float duracaoDoTremor = 0.4f;

    // O clarao branco que cobre a tela no instante da explosao, para o HUD ler.
    public const float duracaoDoClarao = 0.25f;
    public static float claraoAte = -1f;

    // Quando o jogador tenta sem ter como pagar, o HUD mostra quanto falta por um
    // instante. Sem esse aviso o K parece quebrado.
    public static float avisoDeSaldoAte = -1f;
    public static int faltouNoAviso = 0;

    private CinemachineImpulseSource tremor;

    // Para o rocket e o colete tremerem a mesma camera sem montar outra fonte de
    // tremor: a camera tem um ouvinte so, e ele ja esta ligado a esta.
    private static Granada instancia;

    void Awake()
    {
        instancia = this;
        // Static sobrevive ao recarregamento da cena: sem isto a partida nova podia
        // nascer com o clarao da anterior ainda aceso.
        claraoAte = -1f;
        avisoDeSaldoAte = -1f;
    }

    void Start()
    {
        // A fonte do tremor e o ouvinte sao montados em codigo pelo mesmo motivo das
        // paredes do Cerco: dois componentes a menos para esquecer de arrastar.
        //
        // Todos os valores vao escritos a mao: componente criado em codigo durante o
        // jogo nasce com tudo zerado, sem os padroes que o Inspector preencheria. Com
        // canal 0 e ganho 0 a camera simplesmente nao treme, sem erro nenhum.
        tremor = gameObject.AddComponent<CinemachineImpulseSource>();
        tremor.ImpulseDefinition.ImpulseChannel = 1;
        tremor.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        tremor.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        tremor.ImpulseDefinition.ImpulseDuration = duracaoDoTremor;
        tremor.DefaultVelocity = Vector3.down;

        // O ouvinte mora na camera virtual. Ele aplica o tremor DEPOIS do Body, entao
        // o CameraNoCerco nao anula a sacudida quando a camera esta presa na parede.
        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();
        if (cam != null && cam.GetComponent<CinemachineImpulseListener>() == null)
        {
            CinemachineImpulseListener ouvinte = cam.gameObject.AddComponent<CinemachineImpulseListener>();
            ouvinte.ChannelMask = 1;
            ouvinte.Gain = 1f;
            ouvinte.Use2DDistance = true;
            ouvinte.ApplyAfter = CinemachineCore.Stage.Aim;
        }
    }

    void Update()
    {
        if (GameManager.estado != Estado.Jogando) return;

        Keyboard kb = Keyboard.current;
        if (kb == null || kb.kKey.wasPressedThisFrame == false) return;

        // Morto nao joga granada: nos 1,45s da animacao de morte o estado ainda e
        // Jogando, e comprar a propria salvacao depois de morto seria trapaca.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerScript ps = (player != null) ? player.GetComponent<PlayerScript>() : null;
        if (ps != null && ps.isMorto) return;

        Lancar();
    }

    // Publico para poder ser chamado de fora do teclado: e por aqui que o teste
    // automatizado joga a granada sem precisar de alguem apertando o K.
    public void Lancar()
    {
        EnemyDeath[] alvos = AlvosNoCerco();

        // Sem ninguem para matar, nao cobra. Pagar 10 moedas por uma explosao no
        // vazio (entre hordas, por exemplo) seria punir o jogador por apertar a tecla
        // errada, e nao pela escolha de ganancia que a granada existe para criar.
        if (alvos.Length == 0) return;

        if (GameManager.Gastar(custo) == false)
        {
            faltouNoAviso = custo - GameManager.moedas;
            avisoDeSaldoAte = Time.time + 1.5f;
            return;
        }

        for (int i = 0; i < alvos.Length; i++)
        {
            // Conta abate (senao a quota da horda nunca fecha), mas NAO solta moeda.
            // Com moeda, uma granada no meio de 8 rebeldes pagaria de volta o proprio
            // preco, e a punicao do item de 50 pontos desapareceria.
            alvos[i].Morrer(false);
        }

        // As balas do fuzileiro em voo somem junto: morrer para um tiro disparado por
        // alguem que acabou de explodir pareceria injusto.
        BulletScript[] balas = FindObjectsByType<BulletScript>(FindObjectsSortMode.None);
        for (int i = 0; i < balas.Length; i++)
        {
            if (balas[i].isBalaInimiga) Destroy(balas[i].gameObject);
        }

        claraoAte = Time.time + duracaoDoClarao;
        if (tremor != null) tremor.GenerateImpulseWithForce(forcaDoTremor);
    }

    // Um tremor menor, pedido de fora. A granada continua sendo o maior da tela.
    public static void Tremer(float forca)
    {
        if (instancia == null || instancia.tremor == null) return;
        instancia.tremor.GenerateImpulseWithForce(forca);
    }

    // So quem esta dentro das paredes. Com o cerco aberto nao existe "dentro", entao
    // vale todo inimigo vivo - na pratica, entre hordas, nao ha nenhum.
    EnemyDeath[] AlvosNoCerco()
    {
        GameObject[] vivos = GameObject.FindGameObjectsWithTag("Enemy");
        System.Collections.Generic.List<EnemyDeath> dentro = new System.Collections.Generic.List<EnemyDeath>();

        for (int i = 0; i < vivos.Length; i++)
        {
            float x = vivos[i].transform.position.x;
            if (Cerco.fechado && (x < Cerco.limiteEsquerdo || x > Cerco.limiteDireito)) continue;

            EnemyDeath ed = vivos[i].GetComponent<EnemyDeath>();
            if (ed != null && ed.isMorto == false) dentro.Add(ed);
        }

        return dentro.ToArray();
    }
}
