using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float altura = 2.5f;
    [SerializeField] private float velocidad = 1.2f;
    [SerializeField] private float fase = 0f;

    private Vector3 posicionInicial;

    public void Configurar(float nuevaAltura, float nuevaVelocidad, float nuevaFase)
    {
        altura = nuevaAltura;
        velocidad = nuevaVelocidad;
        fase = nuevaFase;
    }

    private void Awake()
    {
        posicionInicial = transform.position;
    }

    private void FixedUpdate()
    {
        if (!GameBootstrap.JuegoActivo) return;

        Vector3 destino = posicionInicial;
        destino.y += Mathf.Sin((Time.time + fase) * velocidad) * altura;
        transform.position = destino;
    }
}
