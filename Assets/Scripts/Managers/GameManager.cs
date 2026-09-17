using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public GridPlacement gridPlacement;
    [SerializeField] private GamePhase gamePhase;
    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Test, eventually there needs to be code to check if all
        // players spawned in.
        ChangeGamePhaseServerRpc(gamePhase);
    }

    void Update()
    {
        if (!IsServer) return;
    }

    public void ChangeToCombat()
    {
        ChangeGamePhaseServerRpc(GamePhase.Combat);
    }
    public void ChangeToBuilding()
    {
        ChangeGamePhaseServerRpc(GamePhase.Building);
    }
    [Rpc(SendTo.Server)]
    private void ChangeGamePhaseServerRpc(GamePhase gp)
    {
        gamePhase = gp;
        switch (gamePhase)
        {
            case GamePhase.Combat:
                CamControlClientRpc(true);
            break;

            case GamePhase.Building:
                CamControlClientRpc(false);
            break;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CamControlClientRpc(bool toFps)
    {
        gridPlacement.gameObject.SetActive(!toFps);

        Player player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();

        player.ToggleFirstPerson(toFps);
    }

}

public enum GamePhase
{
    Combat,
    EndOfCombat,
    Building
}
