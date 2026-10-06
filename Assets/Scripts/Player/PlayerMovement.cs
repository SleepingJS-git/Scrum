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

    [Header("Sliding")]
    public float slideSpeed;
    public float slideDeceleration;
    public float slideMinSpeed;
    public float slideDuration;
    public float slideCooldown;

    [Header("Wall Kicking")]
    public float kickReach;
    public float kickUpForce;
    public float kickHorizForce;
    public float wallKickCooldown;

    // Private Variables
    // Current Move State
    private MovementState moveState => (MovementState) main.MoveState.Value;
    private bool isChangingState = false;

    // Horizontal movement
    private bool isMoving;
    private Vector3 velocity;       // Actual Velocity    
    private Vector3 moveDir;        // Movement Direction
    private bool isSprinting = false;

    // Crouching
    private float defaultHeight;
    private float targetCamLevel;
    private bool isCrouchHolding;

    // Sliding
    private Vector3 slideDirection;
    private float slideVelocity;
    private float slideTimer;
    private float slideElapsed;

    // Wall Kicking
    private int wallKicks;
    private float wallKickElapsed;

    // Components
    private CharacterController cc; 
    private PlayerBody body;
    private Player main;
    public void Init(bool isOwner)
    {
        cc = GetComponent<CharacterController>();
        main = GetComponent<Player>();

        isMoving = false;
        defaultHeight = cc.height;

        if (isOwner) 
        {
            body = GetComponent<PlayerBody>();
        }
    }

    #region Basic Movement
    /// <summary>
    /// This does the movement for vertical (walking) and horizontal (gravity)
    /// </summary>
    /// <param name="moveInput"></param>
    public void Move(Vector3 moveInput)
    {
        if (!cc.enabled) return;
        if (moveState == MovementState.Sliding)
        {
            SlideMovement();
            return;
        }

        isMoving = moveInput.sqrMagnitude > 0.0001f;
        if (isMoving) Sprint();

        // The actual direction relative to the way the player is facing
        moveDir = transform.right * moveInput.x + transform.forward * moveInput.z;
        moveDir.Normalize();
        
        // Cache the current speed
        currentSpeed = DetermineMoveSpeed();

        Vector3 targetVelocity = currentSpeed * moveDir;
        
        if (cc.isGrounded) wallKicks = 0;

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

    private void SlideMovement()
    {
        slideVelocity = Mathf.MoveTowards(
            slideVelocity,
            0f,
            slideDeceleration * Time.deltaTime
        );

        Vector3 horizontalVelocity = slideDirection * slideVelocity;

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        if (cc.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y -= gravityScale * Time.deltaTime;
        }

        cc.Move(velocity * Time.deltaTime);

        slideTimer += Time.deltaTime;

        if (slideVelocity <= slideMinSpeed || slideTimer >= slideDuration)
        {
            main.ChangeMoveStateServerRpc((int) MovementState.Crouching);
            slideElapsed = Time.time;
        }
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
    #endregion

    #region Advanced Movement
    public void WallKick()
    {
        if (cc.isGrounded) return;
        if (Time.time < wallKickElapsed + wallKickCooldown) return;

        if (Physics.Raycast(main.PlayerCam.position, main.PlayerCam.forward, out RaycastHit hit, kickReach, Layer.BulletSurfaces, QueryTriggerInteraction.Ignore))
        {
            Vector3 direction;
            if (wallKicks == 0)
            {
                float y = Mathf.Sqrt(kickUpForce * 2f * gravityScale);
                direction = -main.PlayerCam.forward * kickHorizForce;
                direction.y += y;
                velocity = direction;
            }
            else
            {
                float y = Mathf.Sqrt(kickUpForce / (wallKicks + 1) * 2f * gravityScale);
                direction = -main.PlayerCam.forward;
                direction.y = y;
                velocity += direction;
            }

            wallKicks++;
            wallKickElapsed = Time.time;
        }

    }
    public void Jump()
    {
        if (cc.isGrounded)
        {
            if (moveState == MovementState.Crouching)
            {
                main.ChangeMoveStateServerRpc((int) MovementState.Standing);
            }
            else if (moveState == MovementState.Sliding)
            {
                main.ChangeMoveStateServerRpc((int) MovementState.Standing);
                velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravityScale);
            }
            else
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravityScale);
            }
        }
    }

    public void OnSprint(bool isSprinting)
    {
        this.isSprinting = isSprinting;
    }

    private void Sprint()
    {
        if (isSprinting)
        {
            if (moveState == MovementState.Crouching)
            {
                main.ChangeMoveStateServerRpc((int) MovementState.Standing);
            }
            else if (moveState == MovementState.Standing)
            {
                main.ChangeMoveStateServerRpc((int) MovementState.Sprinting);
            }
            else if (moveState == MovementState.Sprinting)
            {
                // main.Body.Play("IsSprinting", true);
            }
        }
        else
        {
            if (moveState == MovementState.Standing) return;
            if (moveState == MovementState.Crouching) return;
            main.ChangeMoveStateServerRpc((int) MovementState.Standing);
            // main.Body.Play("IsSprinting", false);
        }
    }
    public void CrouchHold(bool isCrouching)
    {
        if (isCrouching)
        {
            if (moveState == MovementState.Standing)
            {
                main.ChangeMoveStateServerRpc((int) MovementState.Crouching);
            }
            else if (moveState == MovementState.Sprinting)
            {
                if (Time.time < slideElapsed + slideCooldown) return;
                slideDirection = moveDir;
                // slideVelocity = Mathf.Max(
                //     new Vector3(velocity.x, 0f, velocity.z).magnitude,
                //     slideSpeed
                // );
                slideVelocity = new Vector3(velocity.x, 0f, velocity.z).magnitude + slideSpeed;
                slideTimer = 0f;
                main.ChangeMoveStateServerRpc((int) MovementState.Sliding);
            }
        }
        else
        {
            if (moveState == MovementState.Crouching)
            {
                main.ChangeMoveStateServerRpc((int) MovementState.Standing);
            }
        }
    }
    public void CrouchOrSlide()
    {
        if (isChangingState) return;
        if (moveState == MovementState.Standing)
        {
            main.ChangeMoveStateServerRpc((int) MovementState.Crouching);
        }
        else if (moveState == MovementState.Crouching)
        {
            main.ChangeMoveStateServerRpc((int) MovementState.Standing);
        }
        else if (moveState == MovementState.Sprinting)
        {
            if (Time.time < slideElapsed + slideCooldown) return;
            slideDirection = moveDir;
            // slideVelocity = Mathf.Max(
            //     new Vector3(velocity.x, 0f, velocity.z).magnitude,
            //     slideSpeed
            // );
            slideVelocity = new Vector3(velocity.x, 0f, velocity.z).magnitude + slideSpeed;
            slideTimer = 0f;
            main.ChangeMoveStateServerRpc((int) MovementState.Sliding);
        }
    }
    public void Slide()
    {
        if (!main.IsOwner)
        {
            cc.height = crouchHeight;
            // body.Play("IsCrouching", true);
        }
        else
            StartCoroutine(CrouchRoutine(true));
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
    #endregion
}
public enum MovementState
{
    Standing = 0,   // Standing or Walking
    Crouching = 1,  // Crouching or Crouch Walking
    Sprinting = 2,  // Standing + Sprinting
    Sliding = 3     // Sliding
}