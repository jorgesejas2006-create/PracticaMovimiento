using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float jumpForce = 7.0f;

    [Header("Jugador")]
    [SerializeField] private int numeroJugador = 1;

    private Rigidbody rB;

    void Start()
    {
        rB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Saltar();
    }

    void FixedUpdate()
    {
        Mover();
    }

    private void Mover()
    {
        Vector2 movimiento = Vector2.zero;
        if (numeroJugador == 1)
        {
            if (Keyboard.current.wKey.isPressed)
                movimiento.y += 1;

            if (Keyboard.current.sKey.isPressed)
                movimiento.y -= 1;

            if (Keyboard.current.aKey.isPressed)
                movimiento.x -= 1;

            if (Keyboard.current.dKey.isPressed)
                movimiento.x += 1;
        }

        if (numeroJugador == 2)
        {
            if (Keyboard.current.upArrowKey.isPressed)
                movimiento.y += 1;

            if (Keyboard.current.downArrowKey.isPressed)
                movimiento.y -= 1;

            if (Keyboard.current.leftArrowKey.isPressed)
                movimiento.x -= 1;

            if (Keyboard.current.rightArrowKey.isPressed)
                movimiento.x += 1;
        }

        Vector3 direccion = new Vector3(
            movimiento.x,
            0,
            movimiento.y
        ).normalized;

        rB.linearVelocity = new Vector3(
            direccion.x * playerSpeed,
            rB.linearVelocity.y,
            direccion.z * playerSpeed
        );
    }

    private void Saltar()
    {
        bool presionoSalto = false;

        // Jugador 1 salta con ESPACIO
        if (numeroJugador == 1)
        {
            presionoSalto = Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        // Jugador 2 salta con ENTER
        if (numeroJugador == 2)
        {
            presionoSalto = Keyboard.current.enterKey.wasPressedThisFrame;
        }

        if (presionoSalto && EstaEnSuelo())
        {
            rB.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );
        }
    }

    private bool EstaEnSuelo()
    {
        return Mathf.Abs(rB.linearVelocity.y) < 0.05f;
    }
}