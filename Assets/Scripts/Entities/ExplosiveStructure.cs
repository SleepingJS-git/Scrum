using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(Fracture))]
[RequireComponent(typeof(AudioSource))]
public class ExplosiveStructure : BreakableStructureEntity
{
    public int damage;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private AudioSource explosionAudioSource;
    [SerializeField] private ParticleSystem[] explosionParticles;
    [Tooltip("The amount of time the explosive waits before exploding.")]
    [SerializeField] private float breakDelay;
    private Rigidbody rb;
    public override void OnNetworkSpawn()
    {
        explosionAudioSource = GetComponent<AudioSource>();

        fracture = GetComponent<Fracture>();
        fractureApart = new Action(DelayExplosionRpc);
        AddDeathEvent(fractureApart);
        //AddDeathEvent(Explode);
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalPos = transform.position;
        originalRot = transform.rotation;

        if (GameManager.Instance)
            StartCoroutine(OnRoundStart());
        else
        {
            rb.isKinematic = false;
        }
    }

    /// <summary>
    /// Only the server should call this because OnHit() logic exits only on server
    /// </summary>
    private void ExplodeDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, Layer.ExplosionEffected);
        OnHitData onHitData = new()
        {
            attacker = this, damage = damage, sourceHit = transform.position,
            radius = explosionRadius, explosionForce = explosionForce,
            upwardsModifier = upwardsModifier, damageType = DamageType.Explosion
        };
        foreach (Collider h in hits)
        {
            if (h == null || h.gameObject == gameObject) continue;
            if (h.attachedRigidbody && !h.attachedRigidbody.isKinematic)
            {
                h.attachedRigidbody.AddExplosionForce(
                    onHitData.damage * 10f, 
                    onHitData.sourceHit, 2f, .5f,
                    ForceMode.Impulse);
            }

            Entity e = h.GetComponent<Entity>();
            if (e && e.isAlive.Value)
            {
                e.OnHit(onHitData);
                
                if (e is Player p)
                {
                    Debug.Log("Player Detected");
                    Vector3 explodingForce = PhysicsHelper.ExplosionForce(
                        transform.position,
                        p.transform.position,
                        explosionForce,
                        explosionRadius,
                        upwardsModifier
                    );

                    Debug.Log("Exploding Force Magnitude: " + explodingForce.magnitude);

                    if (explodingForce != Vector3.zero)
                    {
                        p.AddForceRpc(
                            explodingForce.normalized,
                            explodingForce.magnitude,
                            (int) ForceMode.Impulse,
                            RpcTarget.Single(p.OwnerClientId, RpcTargetUse.Temp)
                        );
                    }
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayExplosionSoundRpc()
    {
        explosionAudioSource.Play();
    }

    private void Fracture()
    {
        foreach(Rigidbody rb in fracture.FragmentRoot.GetComponentsInChildren<Rigidbody>())
        {
            rb.AddExplosionForce(
                explosionForce, 
                transform.position, explosionRadius, upwardsModifier,
                ForceMode.Impulse);
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void DelayExplosionRpc()
    {
        StartCoroutine(DelayExplosion());
        fracture.CauseFracture();
        //Fracture();

        if (GameManager.Instance)
            StartCoroutine(Despawn());
        else
            Invoke("ResetBreakable", 3f);
    }

    private IEnumerator DelayExplosion()
    {
        yield return new WaitForSeconds(breakDelay);

        PlayExplosionSoundRpc();

        if (IsServer)
            ExplodeDamage();

        foreach(ParticleSystem p in explosionParticles)
            p.Play();
        Fracture();
    }

    public override void ResetBreakable()
    {
        base.ResetBreakable();
        transform.position = originalPos;
        transform.rotation = originalRot;

        if (GameManager.Instance)
            StartCoroutine(OnRoundStart());
        else
        {
            rb.isKinematic = false;
            Destroy(fracture.FragmentRoot);
        }
    }

    public IEnumerator OnRoundStart()
    {
        Debug.Log("Waiting for combat.");
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Combat; });
        rb.isKinematic = false;
        Debug.Log("Rigidbody physics enabled");
    }
    
    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(5f);
        Destroy(fracture.FragmentRoot);
        gameObject.SetActive(false);
    }
}
