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
    public Transform weaponHolder;      // The transform used to spawn the 3d model in the player's fps pov
    public WeaponRecoil rightHandWeapon;
    public float wallKickCooldown = 0.65f;
    public float kickReach;
    public Action onWallKick;
    private event Action OnPrimaryFire; // An action event that is used when the player is holding the shoot input
    private event Action OnSecondaryFire;
    private bool _canFire;              // Can the player fire their gun?
    private Player _main;               // Reference to main just cuz
    private PlayerHud _hud;             // Reference to the player hud because of ammo and weapon text
    private WeaponType _weaponType;     // Determines the weapon type when the player fires the gun
    private Transform _cam;             // Camera reference for aiming
    private float recoilAmplitude;
    private float wallKickElapsed;
    private float weaponDist;
    public bool KickIsReady => Time.time >= wallKickElapsed + wallKickCooldown;
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
        
        if (_main.IsServer)
        {
            _main.AddDeathEvent(weaponHandler.DropWeapon);
        }

        weaponDist = kickReach;
    }

    /// <summary>
    /// [Called by Client]
    /// Handles if the player is looking at a viable target
    /// </summary>
    public void CheckAimOnTarget()
    {
        Vector3 adjustedHeadPos = _cam.position + (_cam.forward * .65f);
        if (Physics.Raycast(adjustedHeadPos, _cam.forward, weaponDist, Layer.Player, QueryTriggerInteraction.Ignore))
        {
            if (_hud.CurrentCrosshairColor != CrosshairColor.Red)
                _hud.SetCrosshairColor(CrosshairColor.Red);
        }
        else
        {
            if (_hud.CurrentCrosshairColor != CrosshairColor.White)
                _hud.SetCrosshairColor(CrosshairColor.White);
        }

        // If valid surface is within range of kickreach and kick is ready, turn on the brackets
            // else rest does not run 
        // If airborne (_main.Move.IsGrounded), turn on the arrow indicators
            // else arrows are off
        // If aiming at player or buildable, turn the brackets red
            // else brackets are white

        if (!KickIsReady)
        {
            _hud.SetMeleeBracket(CrosshairColor.None, false);
            return;
        }

        if (Physics.Raycast(adjustedHeadPos, _cam.forward, out RaycastHit hit, kickReach, Layer.BulletSurfaces, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Buildable"))
                _hud.SetMeleeBracket(CrosshairColor.Red, !_main.Move.IsGrounded);
            else _hud.SetMeleeBracket(CrosshairColor.White, !_main.Move.IsGrounded);
                
                
        }
        else _hud.SetMeleeBracket(CrosshairColor.None, false);
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
    public void EmptyWeapon()
    {
        // If no weapon, then what are u dropping?
        if (!weaponInHand) return;
        
        _main.Body.Play("HasWeapon", false);
        // Reset Hud Stuff for the client owner only
        if (_main.IsOwner)
        {
            _hud.UpdateWeaponName();
            _hud.UpdateWeaponInfo(-1, -1);
        }
            

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
        // Create the new gun
        weaponInHand = Instantiate(data.weaponInHand, weaponHolder);

        // Give the weapon handler the firing point
        // This client's firing point is on the weapon overlay
        // Other client's firing point is on the 3d body
        weaponHandler.firingPoint = weaponInHand.transform.Find("Firing Point");
        
        _weaponType = data.WeaponType;

        _main.Body.Play("HasWeapon", true);

        // If the weapon is hitscan, give it the normal gun firing behavior
        if (_weaponType == WeaponType.Hitscan)
        {
            OnPrimaryFire = FireWeapon;
            recoilAmplitude = (data as HitscanData).recoilAmplitude;
            rightHandWeapon.SetResetTime();
            // Set up Hud stuff
            _hud.UpdateWeaponName(data.weaponName);
            _hud.UpdateWeaponInfo(c, r);

            weaponDist = 50f;
        }
        else if (_weaponType == WeaponType.Melee)
        {
            OnPrimaryFire = SwingWeapon;
            recoilAmplitude = -(data as MeleeData).swingStrength;
            rightHandWeapon.SetResetTime(data.fireRate);
            // Set up Hud stuff
            _hud.UpdateWeaponName(data.weaponName);
            _hud.UpdateWeaponInfo(-1, -1);
            weaponDist = (data as MeleeData).attackRange;
        }

        rightHandWeapon.AddRecoil(-300);
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
        weaponInHand = Instantiate(data.weaponModel, _main.Body.rightHand);
        weaponHandler.firingPoint = weaponInHand.transform.Find("Firing Point");
        _main.Body.rightArmRig.weight = 1f;
    }

    /// <summary>
    /// Client-side weapon detection.
    /// </summary>
    private void FireWeapon()
    {
        if (weaponHandler.HasNoAmmo)
        {
            weaponHandler.DropWeapon();
            EmptyWeapon();

            // Logic for no bullets in general
            return;
        }
        if (weaponHandler.NeedsReload)
        {
            weaponHandler.DropWeapon();
            EmptyWeapon();
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

    private void SwingWeapon()
    {
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

    public void RightHandRecoil()
    {
        rightHandWeapon.AddRecoil(recoilAmplitude);
    }

    public void MeleeKick()
    {
        if (!KickIsReady) return;

        _main.Body.PlayKick();


        weaponHandler.MeleeKickServerRpc(_cam.position, _cam.forward, kickReach);

        wallKickElapsed = Time.time;
    }

    public void OnWallKick()
    {
        onWallKick?.Invoke();
    }
}
