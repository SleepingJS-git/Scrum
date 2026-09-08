using System;
using UnityEngine;

/// <summary>
/// Everything related to player weapon handling, shooting, etc is all here
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    public Weapon currentWeapon;
    public Transform weaponHolder;
    public WeaponData testWeapon;
    private event Action OnPrimaryFire; 
    private event Action OnSecondaryFire; 
    private bool _canFire;
    private bool isOwner;
    private Transform camTransform;
    private Player main;
    /// <summary>
    /// Initialize Player Combat. If the client is the owner, then save camera transform for shooting and enable firing
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="cam"></param>
    public void Init(bool owner, Transform cam, Player player)
    {
        isOwner = owner;

        if (isOwner)
        {
            camTransform = cam;
            _canFire = true;
            main = player;
            EquipWeapon(testWeapon); // Test Weapon
        }

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
        if (currentWeapon.IsReady)
        {
            currentWeapon.Fire(camTransform.position, camTransform.forward);
        }
    }

    /// <summary>
    /// Creates the weapon given the WeaponData. Then it sets the action for primary firing.
    /// </summary>
    /// <param name="weaponData"></param>
    public void EquipWeapon(WeaponData weaponData)
    {
        // Clear Primary Fire Action
        OnPrimaryFire = null;
        OnSecondaryFire = null;
        currentWeapon = null;   // Replace with Drop Logic Later

        // Create the new gun
        currentWeapon = Instantiate(weaponData.weaponModel, weaponHolder);
        // The weapon is empty, so fill the weapon with its stats.
        currentWeapon.Init(weaponData, main);

        // If the weapon is hitscan, give it the normal gun firing behavior
        if (weaponData.WeaponType == WeaponType.Hitscan)
        {
            OnPrimaryFire = FireWeapon;
        }
    }


}
