using UnityEngine;

public class RotatingObstacle : MonoBehaviour
{
    [SerializeField] private float velocidad = 110f;
    [SerializeField] private Vector3 eje = Vector3.up;

    public void Configurar(float nuevaVelocidad)
    {
        velocidad = nuevaVelocidad;
    }

    private void Update()
    {
        if (!GameBootstrap.JuegoActivo) return;
        transform.Rotate(eje, velocidad * Time.deltaTime, Space.Self);
    }
}
