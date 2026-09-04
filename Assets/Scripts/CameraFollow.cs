using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 distance = new Vector3(0, 0, 0);
    [SerializeField] private float smoth = 5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPosition = transform.position + distance;
        transform.position = Vector3.Lerp(transform.position, cameraPosition, smoth = Time.deltaTime);
    }
}
