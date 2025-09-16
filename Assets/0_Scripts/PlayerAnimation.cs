using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation Components")]
    [SerializeField] private Animator animator;
    
    private PlayerMovement playerMovement;
    private Vector2 lastDirection = Vector2.down; // Default to facing down
    private bool wasMoving = false;
    
    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        
        // Ensure we have required components
        if (animator == null)
        {
            Debug.LogError("PlayerAnimation requires an Animator component!");
        }
        
        if (playerMovement == null)
        {
            Debug.LogError("PlayerAnimation requires a PlayerMovement component!");
        }
        
        // Start with idle front animation
        PlayIdleAnimation(lastDirection);
    }

    void Update()
    {
        if (playerMovement != null && animator != null)
        {
            UpdateAnimation();
        }
    }
    
    private void UpdateAnimation()
    {
        // Get movement direction from PlayerMovement
        Vector2 movementDirection = playerMovement.GetMovementDirection();
        bool isMoving = playerMovement.IsMoving();
        
        // Play appropriate animation based on movement state and direction
        if (isMoving)
        {
            if (!wasMoving) // Just started moving
            {
                PlayWalkAnimation(movementDirection);
                wasMoving = true;
            }
            else if (HasDirectionChanged(movementDirection)) // Direction changed while moving
            {
                PlayWalkAnimation(movementDirection);
            }
        }
        else
        {
            if (wasMoving) // Just stopped moving
            {
                PlayIdleAnimation(lastDirection);
                wasMoving = false;
            }
        }
        
        // Update last direction when moving
        if (isMoving && movementDirection.magnitude > 0.1f)
        {
            lastDirection = movementDirection.normalized;
        }
    }
    
    private bool HasDirectionChanged(Vector2 currentDirection)
    {
        if (currentDirection.magnitude < 0.1f) return false;
        
        // Get the animation direction for current and last input
        Vector2 currentAnimDir = GetAnimationDirection(currentDirection);
        Vector2 lastAnimDir = GetAnimationDirection(lastDirection);
        
        // Check if the animation direction changed
        return currentAnimDir != lastAnimDir;
    }
    
    private Vector2 GetAnimationDirection(Vector2 inputDirection)
    {
        // Convert input direction to one of the 4 cardinal directions
        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))
        {
            // Horizontal movement is stronger
            return inputDirection.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            // Vertical movement is stronger
            return inputDirection.y > 0 ? Vector2.up : Vector2.down;
        }
    }
    
    private void PlayIdleAnimation(Vector2 direction)
    {
        string animationName = GetIdleAnimationName(direction);
        animator.Play(animationName);
    }
    
    private void PlayWalkAnimation(Vector2 direction)
    {
        string animationName = GetWalkAnimationName(direction);
        animator.Play(animationName);
    }
    
    private string GetIdleAnimationName(Vector2 direction)
    {
        // Determine which idle animation to play based on direction
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement is stronger
            return direction.x > 0 ? "Idle_Right" : "Idle_Left";
        }
        else
        {
            // Vertical movement is stronger
            return direction.y > 0 ? "Idle_Up" : "Idle_Front";
        }
    }
    
    private string GetWalkAnimationName(Vector2 direction)
    {
        // Determine which walk animation to play based on direction
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement is stronger
            return direction.x > 0 ? "Walk_Right" : "Walk_Left";
        }
        else
        {
            // Vertical movement is stronger
            return direction.y > 0 ? "Walk_Up" : "Walk_Front";
        }
    }
    
    // Public method to get current facing direction (useful for other scripts)
    public Vector2 GetFacingDirection()
    {
        return lastDirection;
    }
    
    // Public method to force a specific direction (useful for interactions)
    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            lastDirection = direction.normalized;
            PlayIdleAnimation(lastDirection);
        }
    }
}
