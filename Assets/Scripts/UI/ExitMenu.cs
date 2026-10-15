using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class ExitMenu : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private bool visible;

    public static ExitMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _canvasGroup = GetComponent<CanvasGroup>();
        SetExitVisible(false);
    }

    public void ExitToMainMenu()
    {
        LobbyManager.Instance.LeaveToMainMenu();
    }

    private void SetExitVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (visible)
            CloseExitMenu();
        else
            OpenExitMenu();
    }

    private void OpenExitMenu()
    {
        visible = true;
        SetExitVisible(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PlayerManager.DisableAllPlayerControls();
    }

    public void CloseExitMenu()
    {
        visible = false;
        SetExitVisible(false);

        bool isFPS = PlayerManager.POV == PointOfView.FPS;

        Cursor.visible = !isFPS;
        Cursor.lockState = isFPS ? CursorLockMode.Locked : CursorLockMode.Confined;

        PlayerManager.TogglePlayerControls(isFPS);
    }
}