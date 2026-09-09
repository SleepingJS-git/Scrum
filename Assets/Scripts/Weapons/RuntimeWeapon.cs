using UnityEngine;

public class RuntimeWeapon
{
    public WeaponData data;
    public int currentBulletCount;  // Bullets in chamber
    public int totalBulletCount;    // Total Bullets not in chamber
    public RuntimeWeapon(WeaponData d)
    {
        data = d;
        currentBulletCount = d.bulletCount;
        totalBulletCount = (d.magCount - 1) * d.bulletCount;
    }
}
