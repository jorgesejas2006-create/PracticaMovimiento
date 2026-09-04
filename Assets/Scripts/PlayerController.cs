using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 3.0f;
    [SerializeField] private float jumpForce = 10.0f;
    private PlayerInput input;
    private Rigidbody rB;
    void Start()
    {
        input = GetComponent<PlayerInput>();
        rB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
    }
    private void MovePlayer()
    {
        Vector2 move = input.actions["Move"].ReadValue<Vector2>();
        if (gameObject.name.Equals("PlayerCapsule"))
        {
            transform.Translate(new Vector3(move.x, 0, move.y) * playerSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 physicsMove = new Vector3(move.x, 0, move.y).normalized * playerSpeed * Time.deltaTime;
            rB.AddForce(physicsMove, ForceMode.Impulse);
        }
    }
    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && Mathf.Abs(rB.linearVelocity.y)<0.01)
        {
            rB.AddForce(Vector3.up*jumpForce,ForceMode.Impulse);
        }
    }    
}
