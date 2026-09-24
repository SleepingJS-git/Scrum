using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class DebugCanvas : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI debugStates;
    private Player player;
    private BuildCamera buildCam;
    private string _debugText;
    private bool _hasWeapon;
    private string _currentWeapon;
    private string _buildCam;
    private FixedString512Bytes _gridPlacement;
    private WeaponData _wpnData;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out NetworkClient client))
        {
            player = client.PlayerObject.GetComponent<Player>();
            if (player) buildCam = player.buildCam;
            else buildCam = client.PlayerObject.GetComponent<BuildCamera>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        _debugText = "";

        if (player)
        {
            _hasWeapon = player.combat.weaponHandler.hasWeapon;
            if (_hasWeapon)
            {
                if (!_wpnData)
                {
                    _wpnData = WeaponDatabase.GetWeapon(player.combat.weaponHandler.weaponID.Value);
                    return;
                }

                _currentWeapon = $"{_wpnData.name}";
            }
            else
            {
                _wpnData = null;
                _currentWeapon = "None";
            }

            _debugText = 
            $@"Player Client ID: {OwnerClientId}
            Is Server: {IsServer}
            Health: {player.health.Value}
            Is Alive: {player.isAlive.Value}
            Can Move: {player.CanMove}
            Position: {player.transform.position}
            Has Weapon: {_hasWeapon}
            Current Weapon: {_currentWeapon}
            {player.combat.weaponHandler.DebugInfo()}
            ";
        }

        if (buildCam)
        {
            _buildCam = $@"Build Cam: {buildCam}
            {buildCam.DebugInfo()}";

            if (buildCam.gridBuilding)
                _gridPlacement = buildCam.gridBuilding.DebugInfo();
            else _gridPlacement = "Grid Placement: None";
        }
        else _buildCam = "Build Cam: None";
        

        _debugText += _buildCam;
        _debugText += _gridPlacement;

        if (GameManager.Instance)
        {
            _debugText += GameManager.Instance.DebugInfo();
        }
        debugStates.text = _debugText;
    }
}
