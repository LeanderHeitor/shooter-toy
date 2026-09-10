using UnityEngine;

// CameraFollow.cs
// A camera segue o player so na horizontal (a altura fica travada).
public class CameraFollow : MonoBehaviour
{
    public Transform alvo;
    public float suavidade = 6f;

    void Start()
    {
        if (alvo == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) alvo = player.transform;
        }
    }

    void LateUpdate()
    {
        if (alvo == null) return;

        float x = Mathf.Lerp(transform.position.x, alvo.position.x, suavidade * Time.deltaTime);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
