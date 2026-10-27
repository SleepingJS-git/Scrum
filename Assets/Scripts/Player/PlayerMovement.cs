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
    [SerializeField] private JumpAudio jumpAudio; // Audio source that plays when jumping

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
    public float slideCancelDelay;
    [Range(0f, 2f)] public float slideInputDrag;

    [Header("Wall Kicking")]
    public float kickReach;
    public float kickUpForce;
    public float kickHorizForce;
    public float wallKickCooldown;

    [Header("Physics")]
    public float mass;

    // Private Variables
    // Current Move State
    private MovementState moveState => (MovementState)main.MoveState.Value;
    private bool isChangingState = false;

    // Horizontal movement
    private bool isMoving;
    private Vector3 velocity;       // Actual Velocity    
    private Vector3 moveDir;        // Movement Direction
    private bool isSprinting = false;

    // Crouching
    private float defaultHeight;
    public float TargetCamLevel { get; private set; }

    // Sliding
    private Vector3 slideDirection;
    private float slideVelocity;
    private float slideTimer;
    private float slideElapsed;
    [SerializeField] private bool isCrouchHeld;
    [SerializeField] private bool holdToCrouchInput;

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
            TargetCamLevel = main.Look.standCamLevel;
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

        // The actual direction relative to the way the player is facing
        moveDir = transform.right * moveInput.x + transform.forward * moveInput.z;
        moveDir.Normalize();

        if (moveState == MovementState.Sliding)
        {
            SlideMovement();
            return;
        }

        isMoving = moveInput.sqrMagnitude > 0.0001f;
        if (isMoving) Sprint();
        if (moveState == MovementState.Sprinting)
        {
            if (!isMoving) Sprint();
        }
        
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

    /// <summary>
    /// Determine the movespeed based on the MovementState
    /// </summary>
    /// <returns></returns>
    private float DetermineMoveSpeed()
    {
        if (isMoving)
        {
            switch ((MovementState)main.MoveState.Value)
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
    /// Sliding movement. If the player is sliding, Move() calls this method and then doesn't run normal locomotion code.
    /// </summary>
    private void SlideMovement()
    {
        // Get the current sliding velocity after applying deceleration
        slideVelocity = Mathf.MoveTowards(
            slideVelocity,
            0f,
            slideDeceleration * Time.deltaTime
        );

        // Gradually steer the slide toward the player's input.
        if (moveDir.sqrMagnitude > 0.001f)
        {
            slideDirection = Vector3.Slerp(
                slideDirection,
                moveDir,
                slideInputDrag * Time.deltaTime
            ).normalized;
        }
        
        // Slide velocity is dependent on horizontal axis only
        Vector3 horizontalVelocity = slideDirection * slideVelocity;
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        // Apply gravity
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        else
            velocity.y -= gravityScale * Time.deltaTime;

        cc.Move(velocity * Time.deltaTime);


        // Update the slide timer
        // Timer is cumulated via deltaTime. This represents how long the player has been sliding for
        slideTimer += Time.deltaTime;

        // The player can slide cancel after the slide window passes
        if (slideTimer >= slideCancelDelay && !isCrouchHeld)
        {
            FinishSlide(false);
            return;
        }

        // End the slide naturally when its duration or momentum runs out
        if (slideTimer >= slideDuration ||
            slideVelocity <= slideMinSpeed)
        {
            FinishSlide(true);
        }
    }

    /// <summary>
    /// Finish the slide. Determine what happens after slide ends.
    /// </summary>
    /// <param name="isCrouched">True = player ends crouching, False = player is standing</param>
    private void FinishSlide(bool isCrouched)
    {
        body.Play("IsSliding", false);
        body.Play("IsCrouching", isCrouched);

        MovementState nextState = isCrouched
            ? MovementState.Crouching
            : MovementState.Standing;

        main.ChangeMoveStateServerRpc((int)nextState);
        slideElapsed = Time.time;
    }


    /// <summary>
    /// The client controls the animations
    /// </summary>
    void UpdateAnimation()
    {
        body.Play("IsMoving", isMoving);


        if (!isMoving)
        {
            body.CrossBlendLocomotion("Forward", 0f);
            body.CrossBlendLocomotion("Strafe", 0f);
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

    public void StopVelocity()
    {
        velocity = Vector3.zero;
    }
    #endregion

    #region Advanced Movement
    /// <summary>
    /// [Called by Client]
    /// Simulate an applied force to the player.
    /// CharacterController does not have AddForce(), and movement is determined by CharacterController.Move().
    /// </summary>
    /// <param name="dir">Direction of the force</param>
    /// <param name="force">Amount of Force</param>
    /// <param name="forceMode">The Force Mode</param>
    public void AddForce(Vector3 dir, float force, ForceMode forceMode)
    {
        Debug.Log("AddForce ForceMode: " + forceMode);
        switch (forceMode)
        {
            case ForceMode.Impulse:
                velocity += PhysicsHelper.Impulse(dir, force, mass);
            break;
        }
    }
    
    /// <summary>
    /// Temporary WallKick method. If the player is looking at a valid surface, do a wall kick.
    /// </summary>
    public void WallKick()
    {
        if (Time.time < wallKickElapsed + wallKickCooldown) return;
        body.PlayTrigger("Kick");
        // Damage logic can happen during this window
        if (cc.isGrounded) return;

        if (Physics.Raycast(main.PlayerCam.position, main.PlayerCam.forward, out RaycastHit hit, kickReach, Layer.BulletSurfaces, QueryTriggerInteraction.Ignore))
        {
            Vector3 direction;

            // Wall kicks become less effective if the player does multiple consecutive wallkicks without resetting
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

    /// <summary>
    /// Jump
    /// </summary>
    public void Jump()
    {
        if (cc.isGrounded)
        {
            // If the player was crouching, make them stand
            if (moveState == MovementState.Crouching)
            {
                body.Play("IsCrouching", false);
                main.ChangeMoveStateServerRpc((int)MovementState.Standing);
            }

            // If the player was sliding, make them stand and also make them jump
            else if (moveState == MovementState.Sliding)
            {
                body.Play("IsSliding", false);
                main.ChangeMoveStateServerRpc((int)MovementState.Standing);
                velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravityScale);
                jumpAudio.PlayJump();
            }

            // If the player is standing or sprinting, do a normal jump
            else
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravityScale);
                jumpAudio.PlayJump();
            }
        }
    }

    /// <summary>
    /// Player Input for sprinting. Updates the private isSprinting for PlayerMovement.
    /// </summary>
    /// <param name="isSprinting"></param>
    public void OnSprint(bool isSprinting)
    {
        this.isSprinting = isSprinting;
    }

    /// <summary>
    /// Sprinting logic. Called in Move().
    /// </summary>
    private void Sprint()
    {
        // The player is holding the sprint key and is sprinting.
        if (isSprinting)
        {
            // Note (if player is crouching): Because the player is holding sprint key, logic for moveState == MovementState.Crouching runs
            // and then immediately, logic for moveState == MovementState.Standing runs
            // Making the player go from crouching -> standing -> sprint

            // If the player was crouching, make them stand up
            if (moveState == MovementState.Crouching)
            {
                body.Play("IsSprinting", true);
                body.Play("IsCrouching", false);
                main.ChangeMoveStateServerRpc((int)MovementState.Standing);
            }

            // If the player is standing, make them sprint
            else if (moveState == MovementState.Standing)
            {
                body.Play("IsSprinting", true);
                main.ChangeMoveStateServerRpc((int)MovementState.Sprinting);
            }

            // While the player is sprinting, make the animator param "IsSprinting" true cuz they are sprinting
            else if (moveState == MovementState.Sprinting)
            {
                body.Play("IsSprinting", true);
            }
        }

        // Player let go of the sprint key and is no longer sprinting
        else
        {
            // Only when the player's current moveState is Sprinting do we actually change it
            if (moveState == MovementState.Standing) return;
            if (moveState == MovementState.Crouching) return;

            body.Play("IsSprinting", false);
            main.ChangeMoveStateServerRpc((int)MovementState.Standing);
        }
    }

    
    /// <summary>
    /// Crouching logic for players who have the 'Hold to Crouch' setting.
    /// This method is called whenever the player first presses or lets go of the crouch key.
    /// </summary>
    /// <param name="isCrouching">true = input was pressed (ctx.started), false = input was let go (ctx.canceled)</param>
    public void CrouchHolding(bool isCrouching)
    {
        isCrouchHeld = isCrouching;

        if (isChangingState) return;

        // Player is holding crouch key
        if (isCrouching)
        {
            // If player is standing, make them crouch 
            if (moveState == MovementState.Standing)
            {
                body.Play("IsCrouching", true);
                body.Play("IsSprinting", false);
                main.ChangeMoveStateServerRpc((int)MovementState.Crouching);
            }

            // If player was sprinting, slide
            else if (moveState == MovementState.Sprinting)
            {
                StartSlide();
            }
        }

        // Player is not holding crouch key
        else
        {
            // Only if movestate is crouching do we actually change it to standing
            if (moveState == MovementState.Crouching)
            {
                body.Play("IsCrouching", false);
                main.ChangeMoveStateServerRpc((int)MovementState.Standing);
            }
        }
    }


    /// <summary>
    /// For players who have the 'Toggle to Crouch' setting.
    /// This method is called only when the player presses the crouch key.
    /// </summary>
    public void CrouchOrSlide()
    {        
        if (isChangingState) return;

        // If player was standing, make them crouch
        if (moveState == MovementState.Standing)
        {
            isCrouchHeld = true;
            body.Play("IsCrouching", true);
            body.Play("IsSprinting", false);
            main.ChangeMoveStateServerRpc((int)MovementState.Crouching);
        }

        // If player was crouching, make them stand
        else if (moveState == MovementState.Crouching)
        {
            isCrouchHeld = false;
            body.Play("IsCrouching", false);
            main.ChangeMoveStateServerRpc((int)MovementState.Standing);
        }

        // If player was sprinting, make them slide
        else if (moveState == MovementState.Sprinting)
        {
            StartSlide();
        }

        // If player was sliding, set isCrouchHeld to false which will make SlideMovement() cancel the slide
        else if (moveState == MovementState.Sliding)
        {
            isCrouchHeld = false;
        }
    }

    /// <summary>
    /// Attempts to start player sliding.
    /// </summary>
    private void StartSlide()
    {
        // If not grounded, prevent sliding in mid air
        if (!cc.isGrounded) return;

        // Check if slide is on cooldown
        if (Time.time < slideElapsed + slideCooldown) return;

        body.Play("IsSliding", true);

        // The sliding direction is the direction the player is moving at that moment.
        slideDirection = moveDir;

        // Get the initial slide velocity
        slideVelocity = new Vector3(velocity.x, 0f, velocity.z).magnitude + slideSpeed;
        slideTimer = 0f;
        
        main.ChangeMoveStateServerRpc((int)MovementState.Sliding);
    }

    /// <summary>
    /// Transition method for when player's movement state is first being changed to Sliding
    /// </summary>
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

    /// <summary>
    /// Transition method for when player's movement state is first being changed to Crouching
    /// </summary>
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

    /// <summary>
    /// Transition method for when player's movement state is crouching and then changed to a move state that would uncrouch
    /// </summary>
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

    /// <summary>
    /// Coroutine that shrinks the collider's height and moves the fps cam up or down dependng on toCrouch 
    /// </summary>
    /// <param name="toCrouch">true = player will shrink height and move cam to crouch level, false player will grow height and move cam to standing level</param>
    /// <returns></returns>
    private IEnumerator CrouchRoutine(bool toCrouch)
    {
        isChangingState = true;

        // Increase y velocity to make the player immediately go up when crouching
        // This is to prevent stuttering because as the collider's height increases, its constantly clipping the floor.
        if (!toCrouch && cc.isGrounded)
            velocity.y = Mathf.Sqrt(30f);

        // Set variables for the height of the collider
        float startHeight = cc.height;
        float targetHeight = toCrouch ? crouchHeight : defaultHeight;

        // Get the target cam level
        TargetCamLevel = toCrouch ? main.Look.crouchCamLevel : main.Look.standCamLevel;

        Vector3 pos = main.Look.camHolder.localPosition;
        float startCamLevel = pos.y;

        float t = 0f;
        
        bool ceiling = false;   // Will always be false if toCrouch is true
        while (t < 1)
        {
            // Check if theres a wall or ground above the player only if they are uncrouching.
            if (!toCrouch)
            {
                Vector3 origin = transform.position + new Vector3(0, cc.height / 2, 0);
                ceiling = Physics.Raycast(origin, Vector3.up, 0.2f, Layer.Wall | Layer.Ground);
            }

            // If there is no ceiling yet, continue increasing or decreasing the height.
            if (!ceiling)
            {
                pos.y = Mathf.Lerp(startCamLevel, TargetCamLevel, t);
                main.Look.camHolder.localPosition = pos;
                cc.height = Mathf.Lerp(startHeight, targetHeight, t);

                t += Time.deltaTime / positionChangeSpeed;
            }

            // If there was a ceiling detected, break this loop
            // Meaning the player's height and cam level will be at wherever it stopped.
            else { ceiling = true; break; }

            yield return null;
        }

        // If there was no ceiling detected, set the camHolder pos and cc height to target values
        if (!ceiling)
        {
            pos.y = TargetCamLevel;
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