using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public string message;
    [Tooltip("Enables or disables the interaction event. False = nothing happens")] 
    public bool active = true;
    [SerializeField] private UnityEvent<Player> onInteraction;

    public void OnInteract(Player player)
    {
        if (!active) return;
        onInteraction?.Invoke(player);
    }
}
