using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public MovementState moveState;

    [Header("Ground Movement")]
    public float moveSpeed;         // Movespeed of player
    public float groundAccel, groundDecel;      // Acceleration and Deceleration of speed
    private float currentSpeed;     // Current speed

    [Header("Jumping")]
    public float jumpHeight;        // How high the player jumps
    public float gravityScale;      // Gravity
    public float upwardGravMult;    // How fast the player jumps
    
    [Header("Crouching")]
    public float crouchHeight = 1f;
    private bool isMoving;
    private Vector3 velocity;       // Actual Velocity    
    private Vector3 moveDir;        // Movement Direction
    private CharacterController cc; 
    private PlayerBody body;
    private Player main;
    private float defaultHeight;
    public void Init(bool isOwner)
    {
        moveState = MovementState.Standing;
        cc = GetComponent<CharacterController>();
        isMoving = false;
        defaultHeight = cc.height;

        if (isOwner) 
        {
            body = GetComponent<PlayerBody>();
            main = GetComponent<Player>();
        }
    }

    public void Jump()
    {
        if (cc.isGrounded) velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravityScale);
    }

    public void CrouchOrSlide()
    {
        main.ChangeMoveStateServerRpc();
    }

    public void Crouch()
    {
        if (!main.IsOwner)
        {
            cc.height = crouchHeight;
            // body.Play("IsCrouching", true);
        }
        else
            LocalCrouch();
    }

    public void UnCrouch()
    {
        cc.height = defaultHeight;
    }

    private void LocalCrouch()
    {
        cc.height = crouchHeight;
    }

    /// <summary>
    /// This does the movement for vertical (walking) and horizontal (gravity)
    /// </summary>
    /// <param name="moveInput"></param>
    public void Move(Vector3 moveInput)
    {
        if (!cc.enabled) return;
        isMoving = moveInput.sqrMagnitude > 0.0001f;

        // The actual direction relative to the way the player is facing
        moveDir = transform.right * moveInput.x + transform.forward * moveInput.z;
        moveDir.Normalize();
        
        // Cache the current speed
        currentSpeed = isMoving ? moveSpeed : 0f;

        Vector3 targetVelocity = currentSpeed * moveDir;
        

        // If is grounded, accelerate if moving or decelerate if not
        float accel = isMoving ? groundAccel : groundDecel;

        float velY = velocity.y;
        targetVelocity.y = 0f;
        velocity.y = 0f;

        // Accelerate current velocity to target
        velocity = Vector3.MoveTowards(velocity, targetVelocity, accel * Time.deltaTime);
        velocity.y = velY;

        // Gravity
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        else
        {
            if (velocity.y > 0f) velocity.y -= gravityScale * upwardGravMult * Time.deltaTime;
            else velocity.y -= gravityScale * Time.deltaTime;
        }
        cc.Move(velocity * Time.deltaTime);
        
        UpdateAnimation();
    }

    /// <summary>
    /// The client controls the animations
    /// </summary>
    void UpdateAnimation()
    {
        body.Play("IsMoving", isMoving);

        if (!isMoving)
        {
            body.Play("Forward", 0f);
            body.Play("Strafe", 0f);
            return;
        }

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        float forwardAmount = Vector3.Dot(moveDir.normalized, forward);
        float strafeAmount = Vector3.Dot(moveDir.normalized, right);

        body.Play("Forward", forwardAmount);
        body.Play("Strafe", strafeAmount);
    }

    public void OnDeathCollider(bool isDead)
    {
        cc.enabled = !isDead;
        // if (isDead)
        //     cc.excludeLayers += Layer.Player;
        // else cc.excludeLayers -= Layer.Player;
    }
}
public enum MovementState
{
    Standing = 0,   // Standing or Walking
    Crouching = 1,  // Crouching or Crouch Walking
    Sprinting = 2,  // Standing + Sprinting
    Sliding = 3     // Sliding
}