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
}

public enum DamageType
{
    Bullet,         // Hit-scan weapons - Nail Gun, Staple Gun, Chainsaw Teeth Gun
    Explosion,      // Explosives - Obvious
    Electricity,    // Electric Damage - 
    Fire,           // Fire or Burning - Heat Guns, Flame Throwers
    Falling,        // Fall Damage - Fall on yo feet
    Blunt,          // Blunt Force Damage - Melee Hammers, or thrown blunt objects
    Slice,          // Slicing Damage - Knives, Buzzsaws
}

