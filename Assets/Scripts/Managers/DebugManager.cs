using Unity.Netcode;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public DebugType debuggingType;
    
    void Start()
    {
        switch (debuggingType)
        {
            case DebugType.Solo_Fps:
            case DebugType.Solo_Building:
                NetworkManager.Singleton.StartHost();
            break;
        }
        
    }
}

public enum DebugType
{
    Solo_Fps,
    Solo_Building,
    Networked_Fps,
    Networked_Building, // Add more in the future
}
