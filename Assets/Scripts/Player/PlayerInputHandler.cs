using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

/// <summary>
/// Handles all the input and values for the player.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerMovement move;
    private Player _main;
    private PlayerInput playerInput;
    public Vector3 MoveInput => new Vector3(_moveInput.x, 0f, _moveInput.y);
    public bool PrimaryInput => _primaryInput;
    public bool SecondaryInput => _secondaryInput;
    private Vector3 _moveInput;
    private Vector2 _mouseLookInput;
    private Vector2 _gamePadInput;
    private bool _primaryInput;
    private bool _secondaryInput;
    public void Init(bool enableInput)
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = enableInput;
        if (enableInput)
        {
            move = GetComponent<PlayerMovement>();
            _moveInput = Vector3.zero;
            _mouseLookInput = Vector2.zero;
            _main = GetComponent<Player>();
        }
        

    }

    /// <summary>
    /// Input Action Locomotion will call this method whenever WASD or Left Stick is inputted.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnMove(CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Input Action MouseLook will call this method whenever the Mouse is moved.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnMouseLook(CallbackContext ctx)
    {
        _mouseLookInput = ctx.ReadValue<Vector2>();
    }
    /// <summary>
    /// Input Action GamepadLook will call this method whenever the Right Stick is moved.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnGamePadLook(CallbackContext ctx)
    {
        _gamePadInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// MouseLook accumulates the input values because it's not called in Update(). Whenever it is
    /// called in Update(), it passes the input value and then clears itself. Gamepad Input
    /// is added along with it because its a continuous value, so clearing it every frame would
    /// make it so the camera won't move when the stick is held in a direction.
    /// </summary>
    /// <returns></returns>
    public Vector2 LookInput()
    {
        Vector2 input = _mouseLookInput;
        _mouseLookInput = Vector2.zero;
        return input + _gamePadInput;
    }
    /// <summary>
    /// Input Action Jumping will call this method whenever Space or South Button [Gamepad] is inputted.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnJump(CallbackContext ctx)
    {
        if (move != null)
            if (ctx.started)
                move.Jump();
    }

    /// <summary>
    /// Input Action Primary Fire will call this method whenever Left Click or Right Click is inputted.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnPrimaryFire(CallbackContext ctx)
    {
        // Holding down Input
        if (ctx.started)
            _primaryInput = true;

        // Letting Go
        else if (ctx.canceled)
            _primaryInput = false;
    }

    /// <summary>
    /// Input Action Secondary Fire will call this method whenever Left Click or Right Click is inputted.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnSecondaryFire(CallbackContext ctx)
    {
        if (ctx.started)
            _secondaryInput = true;
        else if (ctx.canceled)
            _secondaryInput = false;
    }

    public void OnInteract(CallbackContext ctx)
    {
        if (ctx.started)
        {
            _main.interaction.OnInteract();
        }
    }
}
