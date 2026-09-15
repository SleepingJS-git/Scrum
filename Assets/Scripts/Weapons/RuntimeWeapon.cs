using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Being Deprecated
/// </summary>
public class RuntimeWeapon
{
    public WeaponData data;
    public int currentBulletCount;  // Bullets in chamber
    public int reserveBullets;    // Total Bullets not in chamber
    public NetworkVariable<int> currentBullets;
    public RuntimeWeapon(WeaponData d)
    {
        data = d;
        currentBulletCount = d.bulletCount;
        reserveBullets = (d.magCount - 1) * d.bulletCount;
    }

    public RuntimeWeapon(WeaponData d, int cb, int rb)
    {
        data = d;
        currentBulletCount = cb;
        reserveBullets = rb;
    }
}
