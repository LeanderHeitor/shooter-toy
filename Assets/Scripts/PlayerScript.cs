using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // sistema de input NOVO da Unity (padrao deste projeto)

// PlayerScript.cs
// Controla o Tarma: CORRER, PULAR (so no chao), ATIRAR POR TOQUE e MORRER.
// Tambem avisa o Animator qual animacao tocar (Idle / Run / Shoot / Jump / Death).
public class PlayerScript : MonoBehaviour
{
    // Arraste o Rigidbody2D do proprio personagem aqui (o Start tambem pega sozinho se esquecer).
    public Rigidbody2D myRigidbody;

    // Componente que troca as animacoes.
    public Animator animator;

    // Velocidade da corrida (unidades por segundo). Ajuste no Inspector.
    public float moveSpeed = 5.5f;

    // Forca do pulo. Quanto maior, mais alto ele pula.
    public float jumpStrength = 11f;

    // Fica true quando o personagem esta encostando no chao (pra so pular no chao).
    public bool isGrounded = false;

    // O colisor do chao e teleportado todo quadro pra acompanhar a camera (GroundScroller),
    // e isso faz o Box2D perder o contato por UM passo de fisica de vez em quando.
    // Em vez de confiar no Exit, eu guardo QUANDO foi o ultimo encosto e considero que
    // ele esta no chao por mais um tiquinho depois disso. Some o pisca e ainda perdoa
    // o pulo apertado um triz depois de sair da borda.
    public float tempoDeTolerancia = 0.12f;
    private float ultimoContatoComOChao = -999f;

    // ----- TIRO -----
    public GameObject bulletPrefab;      // arraste o prefab da BALA aqui no Inspector
    public Transform firePoint;          // ponto de onde a bala sai (um filho do player)
    public float tirosPorSegundo = 4f;   // limite de cadencia, mesmo apertando muito rapido

    // ----- ARMAS COMPRADAS -----
    // Nenhuma mata mais: todo inimigo morre com um acerto. Os numeros abaixo mudam
    // so a FORMA do tiro - quantos inimigos um disparo alcanca e com que rapidez.

    // Shotgun: tres chumbos, um subindo, um reto e um descendo. Alcance curto de
    // proposito: e a arma de quem aceita chegar perto, e perto e onde a moeda esta.
    public float aberturaDoLeque = 2.2f;     // subida por segundo dos chumbos de fora
    public float alcanceDaShotgun = 9f;

    // Metralhadora: a unica arma em que segurar o botao repete o tiro.
    public float tirosPorSegundoDaMetralhadora = 10f;
    public float tremidaDaMetralhadora = 0.5f;   // o tiro sai um pouco torto

    // Rocket: lento de sair e lento de voar, mas leva todo mundo em volta do acerto.
    public float tirosPorSegundoDoRocket = 1.5f;
    public float velocidadeDoRocket = 9f;
    public float raioDoRocket = 2.5f;

    // ----- COLETE -----
    // Quando o colete gasta o golpe, o jogador fica intocavel por este tempo. Sem
    // isto o rebelde da faca, que continua encostado, mataria no quadro seguinte e o
    // colete teria servido para nada.
    public float invencivelDepoisDoColete = 1.2f;

    // ----- MORTE E RESPAWN -----
    public float tempoInvencivel = 1.5f;   // piscando e sem tomar dano logo que a partida comeca
    public float tempoAteOFimDeJogo = 1.45f; // a morte agora tem 22 quadros (1,375s); a tela so entra depois

    // Guarda pra qual lado o player esta virado (comeca virado pra direita).
    public bool isFacingRight = true;

    // Referencia ao desenho do personagem, pra poder espelhar (virar) ele e piscar.
    private SpriteRenderer spriteRenderer;

    // Relogio do proximo tiro permitido, e estado da morte.
    private float proximoTiro = 0f;
    // Os inimigos olham isto pra parar de perseguir, do mesmo jeito que os scripts de
    // comportamento deles olham o EnemyDeath.isMorto. Precisa ser publico porque a
    // morte dura 1,45s ANTES da tela de fim de jogo aparecer: nesse tempo o jogo
    // ainda esta no estado Jogando, e sem este aviso o rebelde continua atacando um
    // cadaver bem no momento em que o jogador esta olhando para a tela.
    public bool isMorto = false;
    private float invencivelAte = 0f;

    void Start()
    {
        if (myRigidbody == null) myRigidbody = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // IMPORTANTE: um corpo parado ADORMECE depois de meio segundo, e a Unity para de
        // chamar OnCollisionStay2D em corpo dormindo. Como e o Stay que carimba o ultimo
        // encosto no chao, dormir fazia o isGrounded vencer e o personagem congelava no
        // quadro do pulo. Nao deixar dormir resolve na raiz.
        if (myRigidbody != null) myRigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;

        // Comeca invencivel por um instante pra nao morrer no momento do respawn.
        invencivelAte = Time.time + tempoInvencivel;
    }

    void Update()
    {
        // Enquanto esta morto nao aceita mais comando nenhum: so espera o reinicio da cena.
        if (isMorto == true) return;

        // O Update roda mesmo com o tempo parado - timeScale zero congela a FISICA,
        // nao os scripts. Sem esta linha dava pra atirar de dentro do menu e da pausa,
        // e a tecla que comeca a partida tambem disparava um tiro.
        if (GameManager.estado != Estado.Jogando) return;

        Piscar();

        isGrounded = (Time.time - ultimoContatoComOChao) <= tempoDeTolerancia;

        // "kb" e o teclado atual. Se nao houver teclado, saimos pra nao dar erro.
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // ----- CORRER -----
        float horizontal = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  horizontal = -1f; // A ou seta esquerda
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;  // D ou seta direita
        myRigidbody.linearVelocity = new Vector2(horizontal * moveSpeed, myRigidbody.linearVelocity.y);

        // ----- VIRAR PRO LADO QUE ANDA -----
        if (horizontal > 0f) isFacingRight = true;
        else if (horizontal < 0f) isFacingRight = false;

        if (spriteRenderer != null) spriteRenderer.flipX = !isFacingRight;
        if (firePoint != null)
        {
            float x = isFacingRight ? 0.55f : -0.55f;
            firePoint.localPosition = new Vector3(x, firePoint.localPosition.y, 0f);
        }

        // ----- PULAR -----
        if (kb.spaceKey.wasPressedThisFrame && isGrounded == true)
        {
            myRigidbody.linearVelocity = new Vector2(myRigidbody.linearVelocity.x, jumpStrength);

            // Gasta a tolerancia na hora, senao daria pra pular de novo dentro da janela.
            ultimoContatoComOChao = -999f;
            isGrounded = false;
        }

        // Um tiro por toque em J ou no mouse: segurar nao cria novas balas. A
        // metralhadora e a excecao que o dinheiro compra - com ela, segurar atira.
        bool apertouTiro = kb.jKey.wasPressedThisFrame ||
                          (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
        bool segurandoTiro = kb.jKey.isPressed ||
                            (Mouse.current != null && Mouse.current.leftButton.isPressed);

        bool querAtirar = (Arsenal.arma == TipoDeArma.Metralhadora) ? segurandoTiro : apertouTiro;
        if (querAtirar && Time.time >= proximoTiro)
        {
            // A cadencia e lida ANTES do tiro: o ultimo tiro de uma arma devolve a
            // pistola, e o intervalo dele tem que ser o da arma que o disparou.
            float cadencia = CadenciaDaArma();
            Atirar();
            proximoTiro = Time.time + (1f / cadencia);
        }

        // ----- AVISA O ANIMATOR -----
        if (animator != null)
        {
            animator.SetFloat("speed", Mathf.Abs(horizontal));
            animator.SetBool("isGrounded", isGrounded);
            // Mantem a animacao visivel durante o intervalo, mesmo soltando a tecla.
            animator.SetBool("isShooting", Time.time < proximoTiro);
        }
    }

    // Enquanto esta invencivel, o desenho aparece e some pra avisar o jogador.
    void Piscar()
    {
        if (spriteRenderer == null) return;

        // Fica visivel depois da invencibilidade; durante ela, alterna o desenho.
        spriteRenderer.enabled = Time.time >= invencivelAte ||
                                 Mathf.FloorToInt(Time.time * 10f) % 2 == 0;
    }

    float CadenciaDaArma()
    {
        switch (Arsenal.arma)
        {
            case TipoDeArma.Metralhadora: return tirosPorSegundoDaMetralhadora;
            case TipoDeArma.Rocket:       return tirosPorSegundoDoRocket;
            default:                      return tirosPorSegundo;
        }
    }

    // Dispara conforme a arma que o jogador carrega, e gasta um tiro da municao.
    void Atirar()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Cada toque reinicia o clipe; correndo ou pulando, mantem a animacao de movimento.
        if (animator != null && isGrounded && myRigidbody.linearVelocity.x == 0f)
            animator.Play("Shoot", 0, 0f);

        switch (Arsenal.arma)
        {
            case TipoDeArma.Shotgun:
                // Um toque, tres chumbos: e o unico jeito do jogo de acertar tres
                // rebeldes com um disparo so, desde que eles estejam perto.
                CriarBala(-aberturaDoLeque).alcance = alcanceDaShotgun;
                CriarBala(0f).alcance = alcanceDaShotgun;
                CriarBala(aberturaDoLeque).alcance = alcanceDaShotgun;
                break;

            case TipoDeArma.Metralhadora:
                CriarBala(Random.Range(-tremidaDaMetralhadora, tremidaDaMetralhadora));
                break;

            case TipoDeArma.Rocket:
                BulletScript rocket = CriarBala(0f);
                rocket.moveSpeed = velocidadeDoRocket;
                rocket.raioDaExplosao = raioDoRocket;

                // Maior e laranja: o jogador precisa ver no ar que aquele tiro e
                // diferente, antes de ver o que ele faz.
                rocket.transform.localScale = new Vector3(0.6f, 0.28f, 1f);
                SpriteRenderer sr = rocket.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 0.45f, 0.15f);
                break;

            default:
                CriarBala(0f);
                break;
        }

        Arsenal.GastarTiro();
    }

    // Cria uma bala no firePoint e manda ela pra direcao que o player esta virado.
    BulletScript CriarBala(float subida)
    {
        GameObject bala = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletScript bs = bala.GetComponent<BulletScript>();
        bs.direction = isFacingRight ? 1f : -1f;
        bs.velocidadeVertical = subida;
        return bs;
    }

    // Chamado pelos inimigos (encostao da faca) e pela bala inimiga.
    public void Morrer()
    {
        if (isMorto == true) return;
        if (Time.time < invencivelAte) return; // invencivel logo depois do respawn

        // O colete gasta o golpe no lugar do jogador. Ele continua morrendo com um
        // acerto so - o colete e que levou este. O pisca da invencibilidade e o
        // tremor sao o recibo: sem eles, o jogador nem percebe que acabou de perder
        // o colete e acha que o tiro errou.
        if (Arsenal.AbsorverGolpe())
        {
            invencivelAte = Time.time + invencivelDepoisDoColete;
            Granada.Tremer(0.3f);
            return;
        }


        isMorto = true;

        if (animator != null) animator.SetBool("isMorto", true);
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        // Para de andar e para de colidir, pra nao ser empurrado enquanto cai.
        myRigidbody.linearVelocity = Vector2.zero;
        myRigidbody.simulated = false;

        Collider2D meuColisor = GetComponent<Collider2D>();
        if (meuColisor != null) meuColisor.enabled = false;

        // Espera a animacao de morte terminar e so entao mostra a tela de fim de jogo.
        // Quem recarrega a cena e o GameManager, quando o jogador apertar uma tecla.
        StartCoroutine(EsperarEMostrarFimDeJogo());
    }

    // WaitForSecondsRealtime, e nao WaitForSeconds nem Invoke: os dois ultimos contam
    // tempo ESCALADO. Se o jogador apertasse pausa durante a animacao de morte, o
    // timeScale ia a zero, a espera nunca terminava e o jogo ficava presa na pausa
    // para sempre. O tempo real ignora a pausa e a tela de fim de jogo sempre chega.
    IEnumerator EsperarEMostrarFimDeJogo()
    {
        yield return new WaitForSecondsRealtime(tempoAteOFimDeJogo);
        GameManager.MorreuOJogador();
    }

    // Enter e Stay so carimbam a hora do ultimo encosto. Quem decide se ele esta no
    // chao e a conta la em cima, com a tolerancia.
    private void OnCollisionEnter2D(Collision2D collision) { RegistrarChao(collision); }
    private void OnCollisionStay2D(Collision2D collision) { RegistrarChao(collision); }

    void RegistrarChao(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) ultimoContatoComOChao = Time.time;
    }

    // Nao existe mais OnCollisionExit2D de proposito: era ele que derrubava o
    // isGrounded por um quadro e fazia a animacao piscar pro Jump e voltar.
}
