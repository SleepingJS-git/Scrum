using UnityEngine;

/// <summary>
/// Whenever damage is dealt, OnHit() needs to figure out what exactly
/// hit the entity. This class is exactly that!
/// </summary>
public class OnHitData
{
    public Entity attacker;
    public int damage;
    public Vector3 sourceHit;
    public Vector3 movingDir;
    public float radius;
    public float explosionForce;
    public float upwardsModifier;
    public DamageType damageType;
}

public enum DamageType
{
    Bullet = 0,         // Hit-scan weapons - Nail Gun, Staple Gun, Chainsaw Teeth Gun
    Explosion = 1,      // Explosives - Obvious
    Electricity = 2,    // Electric Damage - 
    Fire = 3,           // Fire or Burning - Heat Guns, Flame Throwers
    Falling = 4,        // Fall Damage - Fall on yo feet
    Blunt = 5,          // Blunt Force Damage - Melee Hammers, or thrown blunt objects
    Slice = 6,          // Slicing Damage - Knives, Buzzsaws
}

