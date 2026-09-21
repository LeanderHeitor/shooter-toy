using UnityEngine;

// O que a horda pede para acabar.
public enum TipoDeHorda
{
    Quota,       // abater uma quantidade de inimigos
    Resistencia  // continuar vivo ate o prazo acabar
}

// Horda.cs
// O relogio da partida. Decide quando uma horda comeca, quando acaba, quanto ela
// paga e de que tamanho fica o cerco.
//
// O DIU2 tinha um spawner que despejava inimigos para sempre, sem comeco nem fim:
// dava para jogar dez minutos sem nunca ter conquistado nada. A horda existe para
// dar ao jogo uma unidade de conquista - um lote com comeco, fim e pagamento - e e
// esse fim que abre o cerco e da acesso a loja.
//
// De cinco em cinco a regra troca: em vez de pedir abates, a horda pede que o
// jogador fique vivo. A troca existe para que a arma comprada na loja encontre, de
// vez em quando, um problema que ela sozinha nao resolve.
public class Horda : MonoBehaviour
{
    // ----- TAMANHO DAS HORDAS -----
    // 8, 12, 16, 20... A conta ignora as hordas de resistencia, senao a primeira
    // horda depois de uma resistencia daria um salto que o jogador leria como
    // injustica.
    public int quotaDaPrimeira = 8;
    public int passoDaQuota = 4;

    // De quantas em quantas hordas vem uma de resistencia.
    public int resistenciaACada = 5;
    public float segundosDeResistencia = 30f;

    // ----- PAGAMENTO -----
    // O bonus de horda limpa e o unico dinheiro que NAO exige ir buscar. E a parte
    // segura da renda: da para contar com ela, e e por isso que ela e pequena perto
    // do que o chao paga.
    public int bonusDeQuota = 5;
    public int bonusDeResistencia = 10;

    // ----- CERCO -----
    public float meiaLarguraDoCerco = 12f;
    public float meiaLarguraNaResistencia = 9f;   // aperta: e a horda mais dificil

    // ----- COMECO DA PROXIMA -----
    // Com o cerco aberto, quem dispara a horda seguinte e o jogador andando para
    // frente. E o unico momento do jogo em que ele escolhe a hora de apanhar, e e
    // nessa pausa que o prisioneiro aparece com a loja.
    public float avancoParaComecar = 7f;

    // Rede de seguranca: se o jogador ficar parado (ou so recuando), a horda comeca
    // sozinha. Sem isto daria para ficar eternamente no intervalo, que e exatamente
    // o tipo de fuga que o cerco foi feito para acabar.
    public float segundosAteComecarSozinha = 12f;

    // ----- ESTADO ATUAL, PARA O HUD LER -----
    public static int numero = 0;            // 0 = a partida ainda nao teve horda
    public static bool emCombate = false;
    public static TipoDeHorda tipo = TipoDeHorda.Quota;
    public static int restam = 0;            // so vale na horda de quota
    public static float segundosRestantes = 0f;  // so vale na de resistencia
    public static int ultimoBonus = 0;       // quanto a ultima horda limpa pagou

    private Spawner spawner;
    private Cerco cerco;
    private Loja loja;
    private Transform jogador;

    // O placar de abates em que esta horda fecha. Guardar o ALVO, e nao quanto ja
    // foi feito, deixa o "restam" ser uma subtracao e nao um contador a manter.
    private int abatesParaFechar = 0;

    // Quantas hordas de quota ja aconteceram. E isto que faz o degrau 8/12/16 ser
    // constante mesmo com as resistencias no meio, sem precisar de uma segunda
    // formula que tenha que concordar com a de EhResistencia para sempre.
    private int hordasDeQuota = 0;

    private float xQuePrecisaAlcancar = 0f;
    private float comecaSozinhaEm = 0f;

    void Awake()
    {
        // Mesma regra do GameManager: static nao morre no recarregamento da cena,
        // entao toda partida precisa zerar isto na mao.
        numero = 0;
        emCombate = false;
        restam = 0;
        segundosRestantes = 0f;
        ultimoBonus = 0;
        hordasDeQuota = 0;

        cerco = GetComponent<Cerco>();
        if (cerco == null) cerco = gameObject.AddComponent<Cerco>();
    }

    void Start()
    {
        spawner = FindFirstObjectByType<Spawner>();
        loja = FindFirstObjectByType<Loja>();

        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) jogador = go.transform;

        if (spawner != null)
        {
            // A linha do chao tem um dono so, e e o Spawner, que a mostra no
            // Inspector. O cerco copia: dois campos que precisam ser iguais na mao
            // um dia ficam diferentes, e o sintoma seria parede flutuando sem erro
            // nenhum no console.
            cerco.alturaDoChao = spawner.alturaDoChao;

            // O spawner so volta a trabalhar quando a primeira horda comecar.
            spawner.Parar();
        }

        AbrirIntervalo();
    }

    void Update()
    {
        // Fora da partida nao existe horda: no menu, na pausa e no fim de jogo o
        // relogio inteiro para junto com o mundo.
        if (GameManager.estado != Estado.Jogando) return;
        if (jogador == null) return;

        if (emCombate) CuidarDoCombate();
        else EsperarOJogadorAvancar();
    }

    void CuidarDoCombate()
    {
        if (tipo == TipoDeHorda.Quota)
        {
            // Nao existe contador proprio de abates da horda: basta comparar o placar
            // da partida com o numero em que ela fecha. Um numero a menos para manter
            // sincronizado, e um lugar a menos para ele dessincronizar.
            restam = abatesParaFechar - GameManager.abates;
            if (restam <= 0) Terminar();
            return;
        }

        segundosRestantes = segundosRestantes - Time.deltaTime;
        if (segundosRestantes <= 0f)
        {
            segundosRestantes = 0f;
            Terminar();
        }
    }

    void EsperarOJogadorAvancar()
    {
        bool avancou = jogador.position.x >= xQuePrecisaAlcancar;
        bool demorou = Time.time >= comecaSozinhaEm;

        if (avancou || demorou) Comecar();
    }

    void Comecar()
    {
        numero = numero + 1;
        tipo = EhResistencia(numero) ? TipoDeHorda.Resistencia : TipoDeHorda.Quota;

        float meiaLargura = (tipo == TipoDeHorda.Resistencia)
            ? meiaLarguraNaResistencia : meiaLarguraDoCerco;

        // O cerco nasce ao redor de onde o jogador esta AGORA. Ele acabou de andar
        // para ca por vontade propria, entao a caixa se fecha onde ele escolheu.
        cerco.Fechar(jogador.position.x, meiaLargura, tipo == TipoDeHorda.Resistencia);

        emCombate = true;

        // O prisioneiro vai embora quando a horda comeca: a loja nunca abre com
        // inimigo vivo.
        if (loja != null) loja.DispensarPrisioneiro();


        if (tipo == TipoDeHorda.Quota)
        {
            hordasDeQuota = hordasDeQuota + 1;
            restam = QuotaDesta();
            abatesParaFechar = GameManager.abates + restam;

            // Nascem exatamente os inimigos da quota: o ultimo a morrer e o ultimo a
            // nascer, e o "restam 1" do HUD nunca mente sobre quantos sobraram.
            if (spawner != null) spawner.ComecarLote(restam);
        }
        else
        {
            segundosRestantes = segundosDeResistencia;

            // Sem limite de quantos nascem: aqui matar nao adianta nada, e essa e
            // justamente a licao da horda.
            if (spawner != null) spawner.ComecarSemLimite();
        }
    }

    void Terminar()
    {
        emCombate = false;

        ultimoBonus = (tipo == TipoDeHorda.Resistencia) ? bonusDeResistencia : bonusDeQuota;
        GameManager.GanharMoedas(ultimoBonus);
        GameManager.RegistrarHorda(numero);

        if (spawner != null) spawner.Parar();

        // Na horda de quota nao sobra ninguem vivo - ela so acaba quando o ultimo
        // morre. Na de resistencia sobra, e quem sobrou recua: deixar inimigos soltos
        // enquanto o jogador faz compras transformaria o intervalo numa emboscada.
        if (tipo == TipoDeHorda.Resistencia) DispensarOsQueSobraram();

        cerco.Abrir();
        AbrirIntervalo();

        // O prisioneiro fica na metade do caminho ate onde a proxima horda comeca.
        // Assim ele esta no caminho natural do jogador: quem quer comprar para nele,
        // quem nao quer passa reto e ja dispara a horda seguinte. A conta mora aqui
        // porque e a Horda quem sabe onde a proxima comeca.
        if (loja != null && jogador != null)
        {
            float x = jogador.position.x + (avancoParaComecar * 0.5f);
            loja.ChamarPrisioneiro(new Vector3(x, cerco.alturaDoChao, 0f));
        }
    }

    void AbrirIntervalo()
    {
        xQuePrecisaAlcancar = jogador != null
            ? jogador.position.x + avancoParaComecar
            : avancoParaComecar;

        comecaSozinhaEm = Time.time + segundosAteComecarSozinha;
    }

    void DispensarOsQueSobraram()
    {
        GameObject[] vivos = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < vivos.Length; i++)
        {
            // Destroy direto, sem passar pelo Morrer(): recuar nao e abate, entao
            // nao conta ponto, nao alimenta a sequencia e nao paga moeda.
            Destroy(vivos[i]);
        }
    }

    bool EhResistencia(int n)
    {
        if (resistenciaACada <= 0) return false;
        return (n % resistenciaACada) == 0;
    }

    // 8, 12, 16, 20 nas quatro primeiras; a quinta e de resistencia e nao tem quota;
    // a sexta retoma em 24. O degrau e constante porque a conta olha para quantas
    // hordas de QUOTA ja aconteceram, e nao para o numero da horda.
    int QuotaDesta()
    {
        return quotaDaPrimeira + (passoDaQuota * (hordasDeQuota - 1));
    }
}
