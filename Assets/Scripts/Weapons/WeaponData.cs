using UnityEngine;

public abstract class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public ulong weaponID;
    public FiringType firingType;
    public WeaponType WeaponType;
    [Header("Weapon Stats")]
    public int damage;
    [Range(0.01f, 1f)] public float fireRate;
    public int bulletCount;
    public int bulletsPerShot;
    public int magCount;
    public float reloadSpeed;
    [Range(0f, 1f), Tooltip("0 means precise - 1 shit ain't landing bruh")] 
    public float bulletSpread;
    [Range(0.005f, 0.1f), Tooltip("Values closer to 0 should be weapons that shoot very fast and have large mags. Closer to 0.1 are for smaller mags")] 
    public float spreadIncreasePerShot;
    [Header("FPS Weapon")]
    public GameObject weaponInHand;
    [Header("Weapon Objects")]
    public GameObject weaponModel;
    public GameObject shootingEffect;
    public AudioClip fireSound;
}

public enum FiringType
{
    Auto,   // Hold down shoot
    Semi,   // Press to shoot once
    Pump,  // Shoot then pump
}

public enum WeaponType
{
    Hitscan,    // Bullets land instantly
    Launcher,   // Shoots a projectile with travel time
    Melee,      // Melee weapon
    Throwing,   // Throw an weapon with trajectory
}

