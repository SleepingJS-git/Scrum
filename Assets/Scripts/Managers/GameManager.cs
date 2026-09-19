using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public BuildingUI buildingUI;
    [SerializeField] private GamePhase gamePhase;
    public static GamePhase GamePhase => Instance.gamePhase;
    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        // Test, eventually there needs to be code to check if all
        // players spawned in.
        // ChangeGamePhaseServerRpc(gamePhase);
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
                CamControlClientRpc(true, (int) gamePhase);
            break;

            case GamePhase.Building:
                CamControlClientRpc(false, (int) gamePhase);
            break;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CamControlClientRpc(bool toFps, int gamePhaseInt)
    {
        gamePhase = (GamePhase) gamePhaseInt;
        buildingUI.gameObject.SetActive(!toFps);

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
