using UnityEngine;

[CreateAssetMenu(fileName = "New Hitscan Weapon", menuName = "Weapons/Hitscan Weapon", order = -1)]
public class HitscanData : WeaponData
{
    public GameObject trailObj;     // This is a placeholder
    public GameObject residue;      // Object that gets left over when it lands
}
