using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Weapons/Melee Weapon", order = -1)]
public class MeleeData : WeaponData
{
    public float attackRange;
    public float hitBoxHeight;
    public float hitBoxWidth;
    public float swingStrength;
    public MeleeDamageType damageType;

    public enum MeleeDamageType
    {
        Blunt = 5,
        Slice = 6
    }
}
