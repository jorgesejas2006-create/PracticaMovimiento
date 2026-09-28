using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 distance = Vector3.zero;
    [SerializeField] private float smoth = 5f;

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 destino = target.position + distance;
        transform.position = Vector3.Lerp(transform.position, destino, smoth * Time.deltaTime);
    }
}
