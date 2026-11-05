using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Weapons/Melee Weapon", order = -1)]
public class MeleeData : WeaponData
{
    public float attackRange;
    public float windupTime;
    public float hitBoxHeight;
    public float hitBoxWidth;
    public float lungeAmount;
    public MeleeDamageType damageType;

    public enum MeleeDamageType
    {
        Blunt = 5,
        Slice = 6
    }
}
