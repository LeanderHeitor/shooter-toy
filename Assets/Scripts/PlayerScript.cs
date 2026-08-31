using UnityEngine;
using UnityEngine.InputSystem; // sistema de input NOVO da Unity (padrao deste projeto)

// PlayerScript.cs
// Controla o personagem: CORRER, PULAR (so no chao) e ATIRAR.
// Tambem AVISA o Animator quando esta correndo ou no ar, pra trocar a animacao.
public class PlayerScript : MonoBehaviour
{
    // Arraste o Rigidbody2D do proprio personagem aqui (o Start tambem pega sozinho se esquecer).
    public Rigidbody2D myRigidbody;

    // Componente que troca as animacoes (Idle / Run / Jump).
    public Animator animator;

    // Velocidade da corrida (unidades por segundo). Ajuste no Inspector.
    public float moveSpeed = 6f;

    // Forca do pulo. Quanto maior, mais alto ele pula.
    public float jumpStrength = 10f;

    // Fica true quando o personagem esta encostando no chao (pra so pular no chao).
    public bool isGrounded = false;

    // ----- TIRO -----
    public GameObject bulletPrefab;  // arraste o prefab da BALA aqui no Inspector
    public Transform firePoint;      // ponto de onde a bala sai (um filho do player)

    // Guarda pra qual lado o player esta virado (comeca virado pra direita).
    public bool facingRight = true;

    // Referencia ao desenho do personagem, pra poder espelhar (virar) ele.
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if (myRigidbody == null) myRigidbody = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // "kb" e o teclado atual. Se nao houver teclado, saimos pra nao dar erro.
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // ----- CORRER -----
        float horizontal = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  horizontal = -1f; // A ou seta esquerda
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;  // D ou seta direita
        myRigidbody.linearVelocity = new Vector2(horizontal * moveSpeed, myRigidbody.linearVelocity.y);

        // ----- VIRAR PRO LADO QUE ANDA -----
        if (horizontal > 0f) facingRight = true;
        else if (horizontal < 0f) facingRight = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight; // espelha o personagem quando vira pra esquerda
        }
        if (firePoint != null)
        {
            float x = facingRight ? 0.6f : -0.6f;
            firePoint.localPosition = new Vector3(x, firePoint.localPosition.y, 0f);
        }

        // ----- AVISAR O ANIMATOR -----
        // speed > 0 => tocar Run;  isGrounded false => tocar Jump.
        if (animator != null)
        {
            animator.SetFloat("speed", Mathf.Abs(horizontal));
            animator.SetBool("isGrounded", isGrounded);
        }

        // ----- PULAR -----
        if (kb.spaceKey.wasPressedThisFrame && isGrounded == true)
        {
            myRigidbody.linearVelocity = new Vector2(myRigidbody.linearVelocity.x, jumpStrength);
        }

        // ----- ATIRAR (tecla J ou clique esquerdo do mouse) -----
        bool atirou = kb.jKey.wasPressedThisFrame;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            atirou = true;
        }
        if (atirou == true)
        {
            Atirar();
        }
    }

    // Cria uma bala no firePoint e manda ela pra direcao que o player esta virado.
    void Atirar()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bala = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletScript bs = bala.GetComponent<BulletScript>();
        if (bs != null)
        {
            bs.direction = facingRight ? 1f : -1f;
        }
    }

    // A Unity chama isto quando o personagem COMECA a encostar em algo.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }

    // A Unity chama isto quando o personagem PARA de encostar em algo.
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = false;
        }
    }
}
