using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Ground Movement")]
    public float walkSpeed;         // Movespeed of player while walking
    public float groundAccel, groundDecel;      // Acceleration and Deceleration of speed
    private float currentSpeed;     // Current speed

    [Header("Jumping")]
    public float jumpHeight;        // How high the player jumps
    public float gravityScale;      // Gravity
    public float upwardGravMult;    // How fast the player jumps
    
    [Header("Crouching")]
    public float crouchSpeed;       // Movespeed of player while crouching
    public float crouchHeight = 1f;
    public float positionChangeSpeed;

    [Header("Sprinting")]
    public float sprintSpeed;

    // Private Variables
    private bool isMoving;
    private Vector3 velocity;       // Actual Velocity    
    private Vector3 moveDir;        // Movement Direction
    private CharacterController cc; 
    private PlayerBody body;
    private Player main;
    private float defaultHeight;
    private float targetCamLevel;
    private bool isChangingState = false;
    private MovementState moveState => (MovementState) main.MoveState.Value;
    [SerializeField] private float crouchTuner;
    public void Init(bool isOwner)
    {
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
        if (cc.isGrounded)
        {
            if (moveState == MovementState.Crouching)
            {
                main.ChangeMoveStateServerRpc(0);
            }
            else
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravityScale);
            }
        }
    }

    public void OnSprint(bool isSprinting)
    {
        if (isSprinting)
        {
            if (moveState == MovementState.Crouching)
            {
                main.ChangeMoveStateServerRpc(0);
            }
            else if (moveState == MovementState.Standing)
            {
                main.ChangeMoveStateServerRpc(2);
            }
            else if (moveState == MovementState.Sprinting)
            {
                // main.Body.Play("IsSprinting", true);
            }
        }
        else
        {
            main.ChangeMoveStateServerRpc(0);
            // main.Body.Play("IsSprinting", false);
        }
        
    }

    public void CrouchOrSlide()
    {
        if (isChangingState) return;
        if (moveState == MovementState.Standing || moveState == MovementState.Sprinting)
        {
            main.ChangeMoveStateServerRpc(1);
        }
        else if (moveState == MovementState.Crouching)
        {
            main.ChangeMoveStateServerRpc(0);
        }
    }

    public void Crouch()
    {
        if (!main.IsOwner)
        {
            cc.height = crouchHeight;
            // body.Play("IsCrouching", true);
        }
        else
            StartCoroutine(CrouchRoutine(true));
    }

    public void UnCrouch()
    {
        if (!main.IsOwner)
        {
            cc.height = defaultHeight;
            // body.Play("IsCrouching", true);
        }
        else
            StartCoroutine(CrouchRoutine(false));
    }

    private IEnumerator CrouchRoutine(bool toCrouch)
    {
        isChangingState = true;
        if (!toCrouch && cc.isGrounded) 
            velocity.y = Mathf.Sqrt(30f);
        float startHeight = cc.height;
        float targetHeight = toCrouch ? crouchHeight: defaultHeight;
        targetCamLevel = toCrouch ? main.Look.crouchCamLevel: main.Look.standCamLevel;
        float t = 0f;
        Vector3 pos = main.Look.camHolder.localPosition;
        float startCamLevel = pos.y;
        bool ceiling = false;
        while (t < 1)
        {
            Vector3 origin = transform.position + new Vector3(0, cc.height / 2, 0);
            ceiling = Physics.Raycast(origin, Vector3.up, 0.2f, Layer.Wall | Layer.Ground) && !toCrouch;

            if (!ceiling)
            {
                pos.y = Mathf.Lerp(startCamLevel, targetCamLevel, t);
                main.Look.camHolder.localPosition = pos;
                cc.height = Mathf.Lerp(startHeight, targetHeight, t);

                t += Time.deltaTime / positionChangeSpeed;
            }
            else { ceiling = true; break; }

            yield return null;
        }

        if (!ceiling)
        {
            pos.y = targetCamLevel;
            main.Look.camHolder.localPosition = pos;

            cc.height = targetHeight;
        }

        isChangingState = false;
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
        currentSpeed = DetermineMoveSpeed();

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

    private float DetermineMoveSpeed()
    {
        if (isMoving)
        {
            switch ((MovementState) main.MoveState.Value)
            {
                case MovementState.Standing:
                return walkSpeed;
                case MovementState.Crouching:
                return crouchSpeed;
                case MovementState.Sprinting:
                return sprintSpeed; // Change to Sprint Speed
            }
        }
        return 0f;
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