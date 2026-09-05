using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Movement")]
    public float moveSpeed;         // Movespeed of player
    public float groundAccel, groundDecel;      // Acceleration and Deceleration of speed
    private float currentSpeed;     // Current speed

    [Header("Jumping")]
    public float jumpHeight;        // How high the player jumps
    public float gravityScale;      // Gravity
    public float upwardGravMult;    // How fast the player jumps

    [Header("Airborne")]
    public float airAccel;
    public float airDecel;    // The acceleration and deceleration of being in the air
    public float airborneDamp;          // Influence of control while in air
    public float maxAirSpeed;           // The max speed while in air to prevent crazy b-hopping

    private bool isMoving;
    private Vector3 velocity;                // Actual Velocity    
    private Vector3 airborneDir;        // Direction moving while airborne
    private CharacterController cc; 
    public void Init()
    {
        cc = GetComponent<CharacterController>();
        isMoving = false;
    }

    /// <summary>
    /// This does the movement for vertical (walking) and horizontal (gravity)
    /// </summary>
    /// <param name="moveInput"></param>
    public void Move(Vector3 moveInput)
    {
        isMoving = moveInput.sqrMagnitude > 0.0001f;

        // The actual direction relative to the way the player is facing
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.z;
        
        // Cache the current speed
        currentSpeed = moveSpeed;

        if (!isMoving) currentSpeed = 0f;

        Vector3 targetVelocity = currentSpeed * moveDir;
        // When we leave the ground, cache the current horizontal velocity to preserve momentum
        if (!cc.isGrounded)
        {
            targetVelocity = airborneDir + (moveDir * airborneDamp);

            // Optional clamp so air control can't stack you into crazy speed
            float max = Mathf.Max(moveSpeed, maxAirSpeed);
            if (targetVelocity.magnitude > max)
                targetVelocity = targetVelocity.normalized * max;
        }

        // Based on movement and grounded, check if we accelerate or decelerate on the ground or in the air
        float accel = cc.isGrounded
            ? (isMoving ? groundAccel : groundDecel)    // If is grounded, accelerate if moving or decelerate if not
            : (isMoving ? airAccel : airDecel); // If not, accelerate in the air if inputting movement or decelerate if not
            airborneDir = moveDir;
        
        // Accelerate current velocity to target
        velocity = Vector3.MoveTowards(velocity, targetVelocity, accel * Time.deltaTime);
    
        // Gravity
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        else
        {
            if (velocity.y > 0f) velocity.y -= gravityScale * upwardGravMult * Time.deltaTime;
            else velocity.y -= gravityScale * Time.deltaTime;
        }
        cc.Move(velocity * Time.deltaTime);
    }
}
