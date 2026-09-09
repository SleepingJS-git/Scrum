using UnityEngine;

public class RuntimeWeapon
{
    public WeaponData data;
    public int currentBulletCount;  // Bullets in chamber
    public int bulletsInMag;    // Total Bullets not in chamber
    public RuntimeWeapon(WeaponData d)
    {
        data = d;
        currentBulletCount = d.bulletCount;
        bulletsInMag = (d.magCount - 1) * d.bulletCount;
    }
}
