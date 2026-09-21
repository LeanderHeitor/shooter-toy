using UnityEngine;

// Spawner.cs
// Cria os inimigos, cada vez mais rapido. E isto que cumpre o "respawn do inimigo"
// do enunciado: ninguem some do jogo, sempre vem mais.
//
// Ele NAO decide mais quando trabalhar. Quem liga e desliga e a Horda, porque agora
// os inimigos vem em lotes com comeco e fim, e nao num fluxo continuo. Sozinho o
// spawner fica parado: sem horda, ninguem nasce.
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

    // Ritmo da Horda de Resistencia. Ali o inimigo nao e um lote a ser limpo, e sim
    // uma torneira aberta: e o ritmo que faz o jogador correr, nao a quantidade.
    public float intervaloDaResistencia = 0.7f;

    // Cada Horda de Resistencia seguinte abre a torneira um pouco mais: a 10 nao pode
    // ser a 5 de novo, senao o jogador que ja venceu uma vez sabe que vence sempre.
    // O minimo existe porque abaixo dele o teto de vivos segura o ritmo de qualquer
    // jeito, e o numero no Inspector mentiria sobre o que acontece no jogo.
    public float encurtaACadaResistencia = 0.15f;
    public float intervaloMinimoDaResistencia = 0.35f;

    // ----- ONDE NASCE -----
    public float segundosParaNascerPelasCostas = 60f;
    public float distanciaDoSpawn = 12f;   // um pouco fora da tela
    public float alturaDoChao = -3.75f;

    // Com o cerco fechado o inimigo nao pode nascer atras da parede, senao ele fica
    // preso do lado de fora e a quota nunca fecha. Empurrar para dentro resolve, mas
    // as vezes empurra para perto demais do jogador: e por isso que existe uma
    // distancia minima abaixo da qual o spawn troca de lado.
    public float distanciaMinimaDoSpawn = 5f;

    // ----- LIGADO PELA HORDA -----
    // Quantos ainda podem nascer nesta horda, e o unico estado que diz se o spawner
    // esta trabalhando: 0 e parado, -1 e sem limite, n e "faltam n". Um campo so
    // porque dois campos que precisam concordar um dia discordam.
    private int aindaPodemNascer = 0;
    private float intervaloForcado = 0f;   // 0 = usa o ritmo que sobe com o tempo

    private float proximoSpawn = 0f;
    private PlayerScript player;
    private bool fuzileiroApresentado;

    void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.GetComponent<PlayerScript>();
    }

    // A horda de quota pede um lote fechado: nascem exatamente estes, e acabou.
    public void ComecarLote(int quantos)
    {
        Ligar(quantos, 0f);
    }

    // A Horda de Resistencia nao tem lote: e uma torneira aberta ate o prazo acabar.
    // O ritmo dela sai daqui mesmo, de quem o tem no Inspector, e nao de quem chama:
    // a Horda so diz qual resistencia e esta (1 = a primeira).
    public void ComecarSemLimite(int qualResistencia)
    {
        float intervalo = intervaloDaResistencia
                        - (encurtaACadaResistencia * (qualResistencia - 1));
        Ligar(-1, Mathf.Max(intervaloMinimoDaResistencia, intervalo));
    }

    public void Parar()
    {
        aindaPodemNascer = 0;
        intervaloForcado = 0f;
    }

    // O primeiro inimigo demora um instante a mais que os outros, para o jogador ter
    // tempo de ver o cerco fechar antes de ser atacado.
    void Ligar(int quantos, float intervalo)
    {
        aindaPodemNascer = quantos;
        intervaloForcado = intervalo;
        proximoSpawn = Time.time + 0.6f;
    }

    void Update()
    {
        if (player == null) return;
        if (aindaPodemNascer == 0) return;
        if (Time.time < proximoSpawn) return;

        proximoSpawn = Time.time + IntervaloAgora();

        // Teto de inimigos vivos ao mesmo tempo.
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxVivos) return;

        Nascer();

        // So desconta quando alguem realmente nasceu: se o teto de vivos barrou o
        // spawn, aquele inimigo ainda esta devendo, e a quota da horda continua
        // fechando certo.
        if (aindaPodemNascer > 0) aindaPodemNascer = aindaPodemNascer - 1;
    }

    // Escolhe de onde o inimigo entra, ja contando com as paredes do cerco.
    //
    // O lado preferido e o que o sorteio pediu, mas dentro de um cerco ele nem sempre
    // cabe: com o jogador encostado numa parede, aquele lado inteiro vira um pedaco
    // estreito colado nele. Nesse caso vale mais o outro lado. E se os dois estiverem
    // apertados - cerco da Resistencia e jogador no canto - fica o mais distante dos
    // dois: nascer perto e ruim, nascer em cima do jogador e morte sem aviso, e o
    // jogo inteiro se apoia em o jogador ver o perigo chegar.
    float OndeNascer(float xDoJogador, float ladoPreferido)
    {
        float preferido = Cerco.ManterDentro(xDoJogador + (ladoPreferido * distanciaDoSpawn));
        if (Mathf.Abs(preferido - xDoJogador) >= distanciaMinimaDoSpawn) return preferido;

        float outro = Cerco.ManterDentro(xDoJogador - (ladoPreferido * distanciaDoSpawn));
        bool outroEMaisLonge = Mathf.Abs(outro - xDoJogador) > Mathf.Abs(preferido - xDoJogador);

        return outroEMaisLonge ? outro : preferido;
    }

    // O tempo entre um inimigo e outro vai encurtando ate o minimo.
    // A Horda de Resistencia ignora essa curva e impoe o proprio ritmo.
    float IntervaloAgora()
    {
        if (intervaloForcado > 0f) return intervaloForcado;

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

        float x = OndeNascer(player.transform.position.x, lado);
        Instantiate(prefab, new Vector3(x, alturaDoChao, 0f), Quaternion.identity);
    }
}
