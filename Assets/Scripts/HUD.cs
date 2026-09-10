using UnityEngine;

// HUD.cs
// Desenha os numeros na tela e, quando o player morre, a tela de fim de jogo.
// Uso o OnGUI com a Comic Sans instalada no sistema, sem precisar montar Canvas.
// O tamanho da letra sai de Screen.height, senao em tela grande o texto fica minusculo.
public class HUD : MonoBehaviour
{
    // A altura da tela dividida por isto vira o tamanho da letra.
    // Em 1080p da 45 pixels; numero menor = letra maior.
    public int divisorDaLetra = 24;

    // A tela de fim de jogo usa uma letra maior que a do placar.
    public float escalaDoFimDeJogo = 1.8f;

    private GUIStyle placar;
    private GUIStyle grande;
    private GUIStyle dica;
    private Font fonte;
    private int tamanhoUsado = -1;

    void OnDestroy() { if (fonte != null) Destroy(fonte); }

    // Monta os estilos uma vez so, e refaz se a janela mudar de tamanho.
    void PrepararEstilos(int tamanho)
    {
        if (tamanhoUsado == tamanho) return;
        tamanhoUsado = tamanho;

        if (fonte == null) fonte = Font.CreateDynamicFontFromOSFont("Comic Sans MS", tamanho);

        placar = new GUIStyle(GUI.skin.label);
        placar.font = fonte; // os estilos grande e dica herdam a mesma Comic Sans
        placar.fontSize = tamanho;
        placar.fontStyle = FontStyle.Bold;
        placar.normal.textColor = Color.white;

        grande = new GUIStyle(placar);
        grande.fontSize = Mathf.RoundToInt(tamanho * escalaDoFimDeJogo);
        grande.alignment = TextAnchor.MiddleCenter;

        // Mesmo tamanho do placar, mas centralizado: e a linha de instrucao do fim de jogo.
        dica = new GUIStyle(placar);
        dica.fontStyle = FontStyle.Normal;
        dica.alignment = TextAnchor.MiddleCenter;
    }

    // Escreve o texto quatro vezes em preto, deslocado, e uma vez em branco por cima.
    // Sem isso o texto branco some nas partes claras do fundo da floresta.
    void Escrever(Rect onde, string texto, GUIStyle estilo, Color cor)
    {
        float d = Mathf.Max(1f, estilo.fontSize * 0.06f);

        Color guardada = estilo.normal.textColor;
        estilo.normal.textColor = Color.black;
        estilo.Draw(new Rect(onde.x - d, onde.y, onde.width, onde.height), texto, false, false, false, false);
        estilo.Draw(new Rect(onde.x + d, onde.y, onde.width, onde.height), texto, false, false, false, false);
        estilo.Draw(new Rect(onde.x, onde.y - d, onde.width, onde.height), texto, false, false, false, false);
        estilo.Draw(new Rect(onde.x, onde.y + d, onde.width, onde.height), texto, false, false, false, false);

        estilo.normal.textColor = cor;
        estilo.Draw(onde, texto, false, false, false, false);
        estilo.normal.textColor = guardada;
    }

    void OnGUI()
    {
        // Desenha texto sem herdar o destaque de mouse dos labels do tema da Unity.
        if (Event.current.type != EventType.Repaint) return;

        int tamanho = Mathf.Max(14, Screen.height / divisorDaLetra);
        PrepararEstilos(tamanho);

        // O placar e desenhado depois do veu para continuar legivel no fim de jogo.
        if (GameManager.isFimDeJogo) DesenharFimDeJogo(tamanho);

        float linha = tamanho * 1.35f;
        float margem = tamanho * 0.6f;
        float largura = Screen.width;

        Escrever(new Rect(margem, margem, largura, linha),
                 "Abates: " + GameManager.abates, placar, Color.white);

        Escrever(new Rect(margem, margem + linha, largura, linha),
                 "Recorde: " + GameManager.recorde, placar, Color.white);

        // A sequencia so aparece quando esta valendo alguma coisa. Ela sumir da tela
        // ja e o aviso de que voce acabou de perde-la.
        if (GameManager.sequencia >= 2)
        {
            Escrever(new Rect(margem, margem + (linha * 2f), largura, linha),
                     "Sequência: " + GameManager.sequencia, placar, new Color(1f, 0.85f, 0.2f));
        }
    }

    // Tela de fim de jogo: simples de proposito. Um veu vermelho e uma frase.
    // Os numeros continuam legiveis no canto, no placar normal, entao repeti-los aqui
    // so encheria a tela.
    void DesenharFimDeJogo(int tamanho)
    {
        Color guardada = GUI.color;

        // Veu vermelho escuro por cima do jogo congelado.
        GUI.color = new Color(0.30f, 0.02f, 0.03f, 0.78f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = guardada;

        float linha = tamanho * 1.5f;
        float meio = (Screen.height * 0.5f) - linha;

        Escrever(new Rect(0f, meio, Screen.width, linha),
                 "VOCÊ MORREU", grande, new Color(1f, 0.25f, 0.2f));

        string textoDica = "aperte qualquer tecla";
        float larguraDica = dica.CalcSize(new GUIContent(textoDica)).x;
        Rect areaDica = new Rect((Screen.width - larguraDica) * 0.5f,
                                 meio + linha * 1.5f, larguraDica, linha);
        Color corDica = areaDica.Contains(Event.current.mousePosition)
            ? Color.white : new Color(0.9f, 0.6f, 0.6f);
        Escrever(areaDica, textoDica, dica, corDica);
    }
}
