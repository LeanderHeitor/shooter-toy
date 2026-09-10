using UnityEngine;

// ParallaxLayer.cs
// Faz uma camada do fundo andar mais devagar que a camera, e a repete pra nunca acabar.
//
// fator = 0   -> a camada gruda na camera (parece parada na tela). E o caso da nevoa e dos raios de luz.
// fator = 1   -> a camada fica parada no mundo (anda junto com o chao). E o caso da camada do mato.
// entre 0 e 1 -> quanto maior, mais rapido a camada passa.
public class ParallaxLayer : MonoBehaviour
{
    public float fator = 0.5f;

    // Largura de UMA repeticao da imagem, em unidades do mundo (largura em pixels / 32).
    public float larguraDoTile = 19.375f;

    private Transform cam;
    private float yInicial;
    private float zInicial;

    void Start()
    {
        if (Camera.main != null) cam = Camera.main.transform;
        yInicial = transform.position.y;
        zInicial = transform.position.z;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Onde a camada deveria estar se andasse na velocidade do parallax.
        float x = cam.position.x * (1f - fator);

        // A imagem se repete, entao empurrar a camada por um numero inteiro de tiles
        // nao muda nada na tela e mantem ela sempre em cima da camera. E isso que deixa infinito.
        if (larguraDoTile > 0f)
        {
            x = x + (Mathf.Round((cam.position.x - x) / larguraDoTile) * larguraDoTile);
        }

        transform.position = new Vector3(x, yInicial, zInicial);
    }
}
