using TMPro;
using UnityEngine;
using Unity.Services.Authentication;

public class PlayerNameSettings : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;

    /// <summary>Displays the player's current name</summary>
    private async void OnEnable()
    {
        // Wait one frame to allow LobbyManager to initialize
        await System.Threading.Tasks.Task.Yield();

        if (!this || !isActiveAndEnabled)
            return;

        if (LobbyManager.Instance == null)
        {
            Debug.LogError("LobbyManager instance is missing.");
            return;
        }

        if (nameInput == null)
        {
            Debug.LogError("Name Input is not assigned in the Inspector.");
            return;
        }

        try
        {
            await LobbyManager.Instance.InitializationTask;

            if (!this || !isActiveAndEnabled)
                return;

            nameInput.text = AuthenticationService.Instance.PlayerName;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>Saves the player's new name</summary>
    public async void ConfirmName()
    {
        try
        {
            await LobbyManager.Instance.InitializationTask;

            // Remove the existing suffix before submitting
            string newName = nameInput.text.Trim().Split('#')[0];

            if (string.IsNullOrWhiteSpace(newName))
                return;

            // Don't update if the name hasn't changed
            string currentName = AuthenticationService.Instance.PlayerName;

            if (newName == currentName.Split('#')[0])
            {
                nameInput.text = currentName;
                return;
            }

            nameInput.text = await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);

            Debug.Log($"Name updated: {nameInput.text}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update name: {e.Message}");
        }
    }
}
