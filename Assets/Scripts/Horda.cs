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
// vez em quando, um problema que ela sozinha nao resolve. Quando o relogio zera,
// entra um chefe, e a horda so acaba quando ele cai.
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

    // Curto de proposito: a resistencia agora e so a abertura. O prato principal
    // da horda e o chefe que entra quando o relogio zera.
    public float segundosDeResistencia = 15f;

    // ----- CHEFES -----
    // Nomes dos prefabs em Assets/Resources, na ordem em que aparecem: a primeira
    // resistencia traz o primeiro, a segunda o segundo, e depois o ciclo recomeca.
    public string[] chefes = { "ChefeMinigun", "ChefeDiCokka" };

    // Quanto tempo depois da queda do chefe o cerco abre. E o tempo das explosoes:
    // abrir no mesmo quadro tiraria o palco da unica cena grande do jogo.
    public float esperaDepoisDoChefe = 1.6f;

    // A horda cujo chefe, ao cair, vence a partida. E a segunda Horda de
    // Resistencia: o Di-Cokka. Dois chefes, dois atos, e o jogo tem um fim.
    public const int hordaDaVitoria = 10;

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

    // Antes a linha de largada era "7 a frente de onde o jogador estava quando a
    // horda acabou". Se ele terminava encostado na parede da esquerda, a linha caia
    // no meio do cerco antigo - justamente onde estavam as moedas - e ir buscar uma
    // delas passando pelo prisioneiro ja disparava a horda seguinte, sem loja.
    // Agora a linha fica sempre alem da parede antiga e alem do prisioneiro: tudo o
    // que sobrou da horda pode ser recolhido, e a loja nunca fica para tras sem querer.
    public float alemDaParedeAntiga = 4f;
    public float alemDoPrisioneiro = 6f;

    // Rede de seguranca: se o jogador ficar parado (ou so recuando), a horda comeca
    // sozinha. Sem isto daria para ficar eternamente no intervalo, que e exatamente
    // o tipo de fuga que o cerco foi feito para acabar. O relogio recomeca sempre
    // que ele avanca: quem esta andando para a frente nao precisa ser empurrado.
    public float segundosAteComecarSozinha = 12f;

    // ----- TESTE -----
    // Apertar T na tela inicial pula direto para uma horda adiantada, ja equipado,
    // para testar sem jogar tudo de novo. Qualquer outra tecla comeca a partida de
    // verdade, pobre e na horda 1.
    public int hordaDoTeste = 4;
    public TipoDeArma armaDoTeste = TipoDeArma.Shotgun;
    public int tirosDoTeste = 20;
    public int coletesDoTeste = 2;
    public int moedasDoTeste = 30;

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
    private float maisLongeNoIntervalo = 0f;   // o x mais a frente que ele ja pisou

    // O chefe da resistencia atual. O bool diz se ele ja foi chamado: o Chefe em
    // si vira nulo quando a carcaca some, e isso nao pode parecer "ainda nao veio".
    private bool chefeEmCampo = false;
    private Chefe chefe;
    private float terminaEm = -1f;

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

    // Chamado pelo GameManager quando o T e apertado no menu. O Awake ja zerou
    // tudo; aqui so se sobrescreve. A partida comeca como se a horda anterior
    // tivesse acabado agora, com a loja aberta.
    public void ComecarNoTeste()
    {
        // O AbrirIntervalo do Start ja rodou com numero 0, que e o certo: com
        // numero > 0 a linha de largada procuraria a parede de um cerco antigo que
        // nesta partida nunca existiu.
        numero = Mathf.Max(0, hordaDoTeste - 1);

        // Conta quantas das hordas ja "jogadas" foram de quota, senao a quota da
        // horda do teste sairia do tamanho da primeira.
        hordasDeQuota = 0;
        for (int n = 1; n <= numero; n++)
            if (EhResistencia(n) == false) hordasDeQuota = hordasDeQuota + 1;

        Arsenal.Equipar(armaDoTeste, tirosDoTeste);
        Arsenal.coletes = Mathf.Min(coletesDoTeste, Arsenal.maximoDeColetes);
        GameManager.moedas = moedasDoTeste;

        if (numero > 0 && loja != null && jogador != null)
        {
            float x = jogador.position.x + (avancoParaComecar * 0.5f);
            loja.ChamarPrisioneiro(new Vector3(x, cerco.alturaDoChao, 0f));
            xQuePrecisaAlcancar = Mathf.Max(xQuePrecisaAlcancar, x + alemDoPrisioneiro);
        }
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

        // Segunda parte da resistencia: o chefe esta em campo e a horda so acaba
        // quando ele cair.
        if (chefeEmCampo)
        {
            if (terminaEm < 0f && (chefe == null || chefe.EstaMorto()))
                terminaEm = Time.time + esperaDepoisDoChefe;

            if (terminaEm > 0f && Time.time >= terminaEm) Terminar();
            return;
        }

        segundosRestantes = segundosRestantes - Time.deltaTime;
        if (segundosRestantes <= 0f)
        {
            segundosRestantes = 0f;
            ChamarChefe();
        }
    }

    // O relogio zerou: a torneira fecha, os rebeldes recuam e entra o chefe. Ele
    // luta sozinho de proposito - com rebelde em volta, a barra de vida viraria
    // mais um numero na tela em vez de ser o assunto da luta.
    void ChamarChefe()
    {
        if (spawner != null) spawner.Parar();
        DispensarOsQueSobraram();

        string nome = PrefabDoChefe(numero);
        GameObject prefab = (nome != null) ? Resources.Load<GameObject>(nome) : null;

        // Sem prefab a resistencia acaba como acabava antes do chefe existir.
        if (prefab == null)
        {
            Terminar();
            return;
        }

        // Entra pela parede mais longe do jogador, para ter espaco de aparecer
        // andando antes de ser um perigo.
        // A folga e o tamanho do proprio chefe: o tanque e largo, e nascer a 1,5 da
        // parede deixava metade dele enterrada nela.
        Chefe modelo = prefab.GetComponent<Chefe>();
        float folga = (modelo != null ? modelo.meiaLarguraDoCorpo : 1f) + 0.3f;

        float x = jogador.position.x;
        bool esquerdaMaisLonge = (x - Cerco.limiteEsquerdo) > (Cerco.limiteDireito - x);
        float xDoChefe = esquerdaMaisLonge ? Cerco.limiteEsquerdo + folga : Cerco.limiteDireito - folga;

        GameObject go = Instantiate(prefab, new Vector3(xDoChefe, cerco.alturaDoChao, 0f), Quaternion.identity);
        chefe = go.GetComponent<Chefe>();
        chefeEmCampo = true;
    }

    void EsperarOJogadorAvancar()
    {
        if (jogador.position.x > maisLongeNoIntervalo + 0.5f)
        {
            maisLongeNoIntervalo = jogador.position.x;
            comecaSozinhaEm = Time.time + segundosAteComecarSozinha;
        }

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
            // justamente a licao da horda. A 10 vem mais rapido que a 5.
            if (spawner != null) spawner.ComecarSemLimite(numero / resistenciaACada);
        }
    }

    void Terminar()
    {
        emCombate = false;
        chefeEmCampo = false;
        chefe = null;
        terminaEm = -1f;

        ultimoBonus = (tipo == TipoDeHorda.Resistencia) ? bonusDeResistencia : bonusDeQuota;
        GameManager.GanharMoedas(ultimoBonus);
        GameManager.RegistrarHorda(numero);

        if (spawner != null) spawner.Parar();

        // A ultima horda nao abre loja nem cerco: nao existe proxima. O bonus entra
        // antes, porque ele tambem conta para a vitoria mais rica.
        if (numero >= hordaDaVitoria)
        {
            DispensarOsQueSobraram();
            GameManager.Venceu();
            return;
        }

        // Na horda de quota nao sobra ninguem vivo - ela so acaba quando o ultimo
        // morre. Na de resistencia sobra, e quem sobrou recua: deixar inimigos soltos
        // enquanto o jogador faz compras transformaria o intervalo numa emboscada.
        if (tipo == TipoDeHorda.Resistencia) DispensarOsQueSobraram();

        cerco.Abrir();
        AbrirIntervalo();

        if (numero == hordaDaVitoria / 2) GameManager.ChegouNaMetade();

        // O prisioneiro fica na metade do caminho ate onde a proxima horda comeca.
        // Assim ele esta no caminho natural do jogador: quem quer comprar para nele,
        // quem nao quer passa reto e ja dispara a horda seguinte. A conta mora aqui
        // porque e a Horda quem sabe onde a proxima comeca.
        if (loja != null && jogador != null)
        {
            float x = jogador.position.x + (avancoParaComecar * 0.5f);
            loja.ChamarPrisioneiro(new Vector3(x, cerco.alturaDoChao, 0f));

            xQuePrecisaAlcancar = Mathf.Max(xQuePrecisaAlcancar, x + alemDoPrisioneiro);
        }
    }

    // A primeira horda de chefe a partir da horda "n": a propria "n" se ela ja for
    // de resistencia, senao a proxima. O HUD usa no fim de jogo para mostrar quem
    // esta esperando o jogador.
    public int HordaDoProximoChefe(int n)
    {
        int arredondada = ((n + resistenciaACada - 1) / resistenciaACada) * resistenciaACada;
        return Mathf.Max(resistenciaACada, arredondada);
    }

    // O nome do prefab do chefe de uma horda de resistencia. Um lugar so para a
    // conta, que o ChamarChefe e o HUD precisam concordar sobre quem vem.
    public string PrefabDoChefe(int hordaDeResistencia)
    {
        if (chefes == null || chefes.Length == 0) return null;
        int qual = ((hordaDeResistencia / resistenciaACada) - 1) % chefes.Length;
        return chefes[Mathf.Max(0, qual)];
    }

    void AbrirIntervalo()
    {
        float x = jogador != null ? jogador.position.x : 0f;
        xQuePrecisaAlcancar = x + avancoParaComecar;

        // Depois da primeira horda existe um cerco antigo, e a linha fica alem dele.
        // O Abrir so desliga as paredes: os limites continuam guardados no Cerco.
        if (numero > 0)
            xQuePrecisaAlcancar = Mathf.Max(xQuePrecisaAlcancar, Cerco.limiteDireito + alemDaParedeAntiga);

        maisLongeNoIntervalo = x;
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
