using System;
using Unity.Netcode;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Position in which the bullets come from (in view of player, not actual)
    public Transform firingPoint;
    public Collider _collider;
    public Rigidbody _rigidBody;
    [Header("Weapon Stats - Don't Touch")]
    public string weaponName;
    public string weaponID;
    public FiringType firingType;
    public WeaponType weaponType;
    public int damage;
    public float fireRate;
    public int currentBullets;
    public int maxBullets;
    public int bulletsPerShot;
    public float reloadSpeed;
    public float bulletSpread;
    public GameObject trailObj;
    public GameObject residue;
    private bool used = false;
    protected float _lastShootTime;
    public bool IsReady { get { return _lastShootTime + fireRate < Time.time; } }
    private OnHitData _onHitData;
    /// <summary>
    /// Initializes the weapon by giving the stats and effects it needs.
    /// </summary>
    /// <param name="data"></param>
    public void Init(WeaponData data, Entity holder)
    {
        // Create OnHitData
        _onHitData = new OnHitData()
        {
            damage = data.damage, attacker = holder
        };

        if (used) return;


        weaponName = data.weaponName;
        weaponID = data.weaponID;
        firingType = data.firingType;
        weaponType = data.WeaponType;
        damage = data.damage;
        fireRate = data.fireRate;
        currentBullets = data.bulletCount;
        maxBullets = data.bulletCount;
        bulletsPerShot = data.bulletsPerShot;
        reloadSpeed = data.reloadSpeed;
        bulletSpread = data.bulletSpread;

        if (data is HitscanData hs)
        {
            trailObj = hs.trailObj;
            residue = hs.residue;
        }

        used = true;
    }

    /// <summary>
    /// A Hitscan bullet starts from the head and lands at whatever they were aiming at.
    /// If it was an entity, then call OnHit.
    /// If it hit something, then call BulletTrail to visualize the shot.
    /// </summary>
    /// <param name="headPos"></param>
    /// <param name="aimDir"></param>
    public void Fire(Vector3 headPos, Vector3 aimDir)
    { 
        // Give the direction a spread.
        Vector3 dir = SpreadRandomizer(aimDir, bulletSpread);

        // Send a raycast from the head to the new dirction within 100 units on only specific layers, and ignoring triggers
        if (Physics.Raycast(headPos, dir, out RaycastHit hit, 100f, Layer.BulletSurfaces, QueryTriggerInteraction.Ignore))
        {
            // If the raycast hit a player, damage the entity. For right now it just
            // damages the entity.
            if (hit.collider.CompareTag("Player"))
            {
                Entity e = hit.collider.GetComponent<Entity>();

                // Check if the entity is alive
                if (e && e.isAlive.Value)
                {
                    // Add where the hit landed for onHitData.
                    _onHitData.sourceHit = hit.point;
                    e.OnHit(_onHitData);
                }
            }

            BulletTrail(firingPoint.transform.position, hit.point);
        }

        // Start the fire rate counter then subtract a shot. 
        _lastShootTime = Time.time;
        currentBullets--;
    }

    /// <summary>
    /// Creates a trail from the firing point to the hit point.
    /// It also instantiates an object when it hits the point.
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    public void BulletTrail(Vector3 startPos, Vector3 endPos)
    {
        Instantiate(residue, endPos, Quaternion.identity);
    }

    /// <summary>
    /// Randomizes the spread of the gun.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="spread"></param>
    /// <returns></returns>
    public Vector3 SpreadRandomizer(Vector3 dir, float spread)
    {
        dir += new Vector3(
            UnityEngine.Random.Range(-spread, spread),
            UnityEngine.Random.Range(-spread, spread),
            UnityEngine.Random.Range(-spread, spread)
        );

        return dir.normalized;
    }
}
