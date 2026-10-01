using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class ExitMenu : MonoBehaviour
{

    private CanvasGroup _canvasGroup;
    private bool visible;
    [SerializeField]
    private PlayerInput playerInput;
    public static ExitMenu Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
        //playerInput = GetComponent<PlayerInput>();
        _canvasGroup = GetComponent<CanvasGroup>();
        SetExitVisible(false);
    }

    private void SetExitVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    // Update is called once per frame
    void Update()
    {
        if (visible)
        {
            //playerInput.actions.FindActionMap("Fps Input").Disable();

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                visible = false;
                SetExitVisible(visible);
                Cursor.visible = false;
            }
        }
        else
        {
            //if(GameManager.GamePhase == GamePhase.Combat)
                //playerInput.actions.FindActionMap("Fps Input").Enable();

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                visible = true;
                SetExitVisible(visible);
                Cursor.visible = true;
            }
        }
    }

}
