using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Everything related to player weapon handling, shooting, etc is all here
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    public Weapon weaponHandler;        // The script that handles the weapon behavior and networking
    public GameObject weaponInHand;     // The game object in the player's fps hand or 3d hand
    public Transform weaponHolder3D;    // The transform used to spawn the 3d model for the other player's pov
    public Transform weaponHolder;      // The transform used to spawn the 3d model in the player's fps pov
    private event Action OnPrimaryFire; // An action event that is used when the player is holding the shoot input
    private event Action OnSecondaryFire;
    private bool _canFire;              // Can the player fire their gun?
    private Player _main;               // Reference to main just cuz
    private PlayerHud _hud;             // Reference to the player hud because of ammo and weapon text
    private WeaponType _weaponType;     // Determines the weapon type when the player fires the gun
    private Transform _cam;             // Camera reference for aiming
    /// <summary>
    /// [Called by Client]
    /// Initialize Player Combat. If the client is the owner, then save camera transform for shooting and enable firing
    /// </summary>
    /// <param name="isOwner"></param>
    /// <param name="h"></param>
    public void Init(bool isOwner, PlayerHud h)
    {
        _canFire = isOwner;
        _main = GetComponent<Player>();
        _hud = h;
        _cam = _main.PlayerCam;
    }

    /// <summary>
    /// [Called by Client]
    /// This handles the left click or right trigger input.
    /// </summary>
    /// <param name="isFiring"></param>
    public void PrimaryInput(bool isFiring)
    {
        if (isFiring && _canFire)
        {
            OnPrimaryFire?.Invoke();
        }
    }

    /// <summary>
    /// [Called by Client] 
    /// Drop the weapon the player is currently holding.
    /// </summary>
    public void DropWeapon()
    {
        // If no weapon, then what are u dropping?
        if (!weaponInHand) return;
        
        // The weapon handler has a server request method.
        // Must request the server to create an empty WeaponDrop
        weaponHandler.DropServerRpc();

        // Reset Hud Stuff
        UpdateWeaponInfo();

        // Clear Primary Fire Action
        OnPrimaryFire = null;
        OnSecondaryFire = null;

        Destroy(weaponInHand);
    }

    /// <summary>
    /// [Called by ClientRpc]
    /// The owner's client will equip a weapon overlay for their screen only
    /// </summary>
    /// <param name="weapon"></param>
    public void EquipWeapon(WeaponData data, int c, int r)
    {
        if (!_main.IsOwner) return;
        DropWeapon();

        // Create the new gun
        weaponInHand = Instantiate(data.weaponInHand, weaponHolder);

        // Give the weapon handler the firing point
        // This client's firing point is on the weapon overlay
        // Other client's firing point is on the 3d body
        weaponHandler.firingPoint = weaponInHand.transform.Find("Firing Point");
        
        _weaponType = data.WeaponType;

        // If the weapon is hitscan, give it the normal gun firing behavior
        if (_weaponType == WeaponType.Hitscan)
        {
            OnPrimaryFire = FireWeapon;
        }

        // Set up Hud stuff
        UpdateWeaponInfo(data.weaponName, $"{c} / {r}");
    }

    /// <summary>
    /// [Called by ClientRpc]
    /// Other clients will call their version of this player to equip a weapon on the
    /// 3D model body
    /// </summary>
    /// <param name="weapon"></param>
    public void EquipWeapon3D(WeaponData data)
    {
        Debug.Log("Equip3D was called");
        weaponInHand = Instantiate(data.weaponModel, weaponHolder3D);
        weaponHandler.firingPoint = weaponInHand.transform.Find("Firing Point");
    }

    /// <summary>
    /// Client-side weapon detection.
    /// </summary>
    private void FireWeapon()
    {
        if (weaponHandler.HasNoAmmo)
        {
            DropWeapon();

            // Logic for no bullets in general
            return;
        }
        if (weaponHandler.NeedsReload)
        {
            DropWeapon();
            return;
        }

        if (weaponHandler.IsReady)
        {
            // Have the weapon handler handle the weapon behavior
            weaponHandler.Shoot(
                _weaponType,
                _cam.position,
                _cam.forward
            );
        }
    }

    /// <summary>
    /// Update the hud text for the weapon name and ammo
    /// </summary>
    /// <param name="nameInfo"></param>
    /// <param name="ammoInfo"></param>
    public void UpdateWeaponInfo(string nameInfo = "", string ammoInfo = "")
    {
        _hud.weaponName.text = nameInfo;
        _hud.ammo.text = ammoInfo;
    }
}
