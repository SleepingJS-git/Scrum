using UnityEngine;

public abstract class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public string weaponID;
    public FiringType firingType;
    public WeaponType WeaponType;
    [Header("Weapon Stats")]
    public int damage;
    [Range(0.01f, 1f)] public float fireRate;
    public int bulletCount;
    public int bulletsPerShot;
    public float reloadSpeed;
    [Range(0f, 1f), Tooltip("0 means precise - 1 shit ain't landing bruh")] 
    public float bulletSpread;
    [Header("Weapon Objects")]
    public Weapon weaponModel;
    public GameObject shootingEffect;
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

