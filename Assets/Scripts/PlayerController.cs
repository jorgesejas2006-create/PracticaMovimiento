using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float jumpForce = 7.0f;

    [Header("Jugador")]
    [SerializeField] private int numeroJugador = 1;
    [SerializeField] private AudioClip sonidoSalto;

    private AudioSource audioSource;
    private Rigidbody rB;

    public int NumeroJugador => numeroJugador;

    private void Start()
    {
        rB = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!GameBootstrap.JuegoActivo) return;
        Saltar();
    }

    private void FixedUpdate()
    {
        if (!GameBootstrap.JuegoActivo || rB == null) return;
        Mover();
    }

    private void Mover()
    {
        if (Keyboard.current == null) return;

        Vector2 movimiento = Vector2.zero;

        if (numeroJugador == 1)
        {
            if (Keyboard.current.wKey.isPressed) movimiento.y += 1;
            if (Keyboard.current.sKey.isPressed) movimiento.y -= 1;
            if (Keyboard.current.aKey.isPressed) movimiento.x -= 1;
            if (Keyboard.current.dKey.isPressed) movimiento.x += 1;
        }
        else if (numeroJugador == 2)
        {
            if (Keyboard.current.upArrowKey.isPressed) movimiento.y += 1;
            if (Keyboard.current.downArrowKey.isPressed) movimiento.y -= 1;
            if (Keyboard.current.leftArrowKey.isPressed) movimiento.x -= 1;
            if (Keyboard.current.rightArrowKey.isPressed) movimiento.x += 1;
        }

        Vector3 direccion = new Vector3(movimiento.x, 0, movimiento.y).normalized;

        rB.linearVelocity = new Vector3(
            direccion.x * playerSpeed,
            rB.linearVelocity.y,
            direccion.z * playerSpeed
        );
    }

    private void Saltar()
    {
        if (Keyboard.current == null || rB == null) return;

        bool presionoSalto = numeroJugador == 1
            ? Keyboard.current.spaceKey.wasPressedThisFrame
            : Keyboard.current.enterKey.wasPressedThisFrame;

        if (presionoSalto && EstaEnSuelo())
        {
            rB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (audioSource != null && sonidoSalto != null)
                audioSource.PlayOneShot(sonidoSalto);
        }
    }

    private bool EstaEnSuelo()
    {
        return Mathf.Abs(rB.linearVelocity.y) < 0.08f;
    }
}
