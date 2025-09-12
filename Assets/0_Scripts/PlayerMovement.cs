using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Input")]
    [SerializeField] private KeyCode upKey = KeyCode.W;
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    
    private Rigidbody2D rb;
    private Vector2 inputVector;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure we have a Rigidbody2D component
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody2D component!");
        }
        
        // Set drag to 0 for instant movement
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void HandleInput()
    {
        // Get input from keyboard
        inputVector.x = 0f;
        inputVector.y = 0f;
        
        if (Input.GetKey(leftKey))
            inputVector.x = -1f;
        else if (Input.GetKey(rightKey))
            inputVector.x = 1f;
            
        if (Input.GetKey(downKey))
            inputVector.y = -1f;
        else if (Input.GetKey(upKey))
            inputVector.y = 1f;
        
        // Normalize diagonal movement to prevent faster movement when moving diagonally
        if (inputVector.magnitude > 1f)
        {
            inputVector = inputVector.normalized;
        }
    }
    
    private void MovePlayer()
    {
        // Apply movement directly to velocity (instant movement)
        rb.linearVelocity = inputVector * moveSpeed;
    }
    
    // Optional: Method to get current movement direction (useful for animations)
    public Vector2 GetMovementDirection()
    {
        return inputVector;
    }
    
    // Optional: Method to check if player is moving
    public bool IsMoving()
    {
        return rb.linearVelocity.magnitude > 0.1f;
    }
}
