using UnityEngine;

// Spawner.cs
// Cria inimigos pra sempre, cada vez mais rapido. E isto que cumpre o
// "respawn do inimigo" do enunciado: ninguem some do jogo, sempre vem mais.
public class Spawner : MonoBehaviour
{
    public GameObject knifePrefab;   // rebelde da faca
    public GameObject riflePrefab;   // rebelde do fuzil

    // ----- RITMO -----
    public float intervaloInicial = 2.5f;
    public float intervaloFinal = 0.6f;
    public float tempoAteORitmoMaximo = 150f;  // 2min30
    public int maxVivos = 6;

    // ----- MISTURA -----
    public float segundosParaOFuzileiroEntrar = 10f;
    public float segundosParaMetadeFuzileiro = 150f;

    // ----- ONDE NASCE -----
    public float segundosParaNascerPelasCostas = 60f;
    public float distanciaDoSpawn = 12f;   // um pouco fora da tela
    public float alturaDoChao = -3.75f;

    private float proximoSpawn = 0f;
    private PlayerScript player;
    private bool fuzileiroApresentado;

    void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.GetComponent<PlayerScript>();

        proximoSpawn = Time.time + intervaloInicial;
    }

    void Update()
    {
        if (player == null) return;
        if (Time.time < proximoSpawn) return;

        proximoSpawn = Time.time + IntervaloAgora();

        // Teto de inimigos vivos ao mesmo tempo.
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxVivos) return;

        Nascer();
    }

    // O tempo entre um inimigo e outro vai encurtando ate o minimo.
    float IntervaloAgora()
    {
        float t = Mathf.Clamp01(Time.timeSinceLevelLoad / tempoAteORitmoMaximo);
        return Mathf.Lerp(intervaloInicial, intervaloFinal, t);
    }

    void Nascer()
    {
        float tempo = Time.timeSinceLevelLoad;

        // ----- QUAL TIPO -----
        // O primeiro spawn com vaga a partir de 10s apresenta o fuzileiro.
        // Depois, a chance cresce de 25% para 50% ate 2min30.
        GameObject prefab = knifePrefab;
        if (riflePrefab != null && tempo >= segundosParaOFuzileiroEntrar)
        {
            float t = Mathf.InverseLerp(segundosParaOFuzileiroEntrar, segundosParaMetadeFuzileiro, tempo);
            if (!fuzileiroApresentado || Random.value < Mathf.Lerp(0.25f, 0.5f, t))
            {
                prefab = riflePrefab;
                fuzileiroApresentado = true;
            }
        }

        if (prefab == null) return;

        // ----- DE QUE LADO -----
        // Ate 60s so aparece pela frente. Depois disso tambem pelas costas.
        float ladoDaFrente = player.isFacingRight ? 1f : -1f;
        float lado = ladoDaFrente;
        if (tempo > segundosParaNascerPelasCostas && Random.value < 0.35f)
        {
            lado = -ladoDaFrente;
        }

        Vector3 posicao = new Vector3(player.transform.position.x + (lado * distanciaDoSpawn),
                                      alturaDoChao, 0f);
        Instantiate(prefab, posicao, Quaternion.identity);
    }
}
