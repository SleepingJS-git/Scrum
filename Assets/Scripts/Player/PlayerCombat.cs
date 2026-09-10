using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Everything related to player weapon handling, shooting, etc is all here
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    public Weapon currentWeapon;
    public Transform weaponHolder;
    private event Action OnPrimaryFire;
    private event Action OnSecondaryFire;
    private bool _canFire;
    private Transform _cam;
    private Player _main;
    private PlayerHud _hud;
    public WeaponDrop weaponDropTemplate; // This will be moved to a singleton manager.
    /// <summary>
    /// Initialize Player Combat. If the client is the owner, then save camera transform for shooting and enable firing
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="cam"></param>
    public void Init(Transform cam, PlayerHud h)
    {
        _cam = cam;
        _canFire = true;
        _main = GetComponent<Player>();
        _hud = h;

    }

    /// <summary>
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
    /// When the weapon is ready to fire (via. fire rate), fire the weapon
    /// </summary>
    public void FireWeapon()
    {
        if (currentWeapon.HasNoAmmo)
        {
            // Logic for no bullets in general
            return;
        }

        if (currentWeapon.NeedsReload)
        {
            DropWeapon();
            return;
        }

        if (currentWeapon.IsReady)
        {
            currentWeapon.Fire(_cam.position, _cam.forward);
            _hud.ammo.text = $"{currentWeapon.currentBullets} / {currentWeapon.reserveBullets}";
        }
    }

    /// <summary>
    /// Creates the weapon given the WeaponData. Then it sets the action for primary firing.
    /// </summary>
    /// <param name="weapon"></param>
    public void EquipWeapon(RuntimeWeapon weapon)
    {
        if (!_main.IsOwner) return;
        DropWeapon();
        Debug.Log("Weapon: " + weapon);

        // Create the new gun
        currentWeapon = Instantiate(weapon.data.weaponTemplate, weaponHolder);
        // The weapon is empty, so fill the weapon with its stats.
        currentWeapon.Init(weapon, _main);

        // If the weapon is hitscan, give it the normal gun firing behavior
        if (currentWeapon.weaponType == WeaponType.Hitscan)
        {
            OnPrimaryFire = FireWeapon;
        }

        // Set up Hud stuff
        Debug.Log("_hud.weaponName" + _hud.weaponName);
        Debug.Log("currentWeapon.weaponName" + currentWeapon.weaponName);
        _hud.weaponName.text = currentWeapon.weaponName;
        _hud.ammo.text = $"{currentWeapon.currentBullets} / {currentWeapon.reserveBullets}";
    }

    public void DropWeapon()
    {
        // If no weapon, then what are u dropping?
        if (!currentWeapon) return;
        
        _main.DropServerRpc(
            currentWeapon.weaponID,
            currentWeapon.currentBullets,
            currentWeapon.reserveBullets,
            _cam.position,
            _cam.rotation
        );

        // Reset Hud Stuff
        _hud.weaponName.text = "";
        _hud.ammo.text = "";

        // Clear Primary Fire Action
        OnPrimaryFire = null;
        OnSecondaryFire = null;

        Destroy(currentWeapon.gameObject);
    }


}
