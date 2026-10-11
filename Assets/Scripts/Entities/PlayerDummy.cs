using Unity.Netcode;
using UnityEngine;

public class PlayerDummy : Entity
{
    [SerializeField]  private PlayerBody _body;
    [SerializeField] private Collider _collider;
    [SerializeField] private AudioSource audioSource;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (_body)
            _body.Init(false);

        if (IsServer)
        {
            AddDeathEvent(DummyDeath);
        }
    }
    public override void OnHit(OnHitData onHitData)
    {
        OnHitClientRpc(onHitData.damage, onHitData.sourceHit, onHitData.upwardsModifier);
        base.OnHit(onHitData);
        // audioSource. play hurt noise
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnHitClientRpc(int damage, Vector3 sourceHit, float upwardsModifier)
    {
        _body.onHitData.damage = damage;
        _body.onHitData.sourceHit = sourceHit;
        _body.onHitData.upwardsModifier = upwardsModifier;
    }

    public void PlayDeathSound()
    {
        // Instead of an Everyone Rpc, just add this method to OnDeathEffects in Inspector.
        int soundIndex = AudioManager.Instance.GetRandomDeathSoundIndex();
        AudioClip clip = AudioManager.Instance.GetDeathSound(soundIndex);
        audioSource.PlayOneShot(clip);
    }
    private void DummyDeath()
    {
        _body.Ragdoll();
        OnDeathCollider(true);
        Invoke(nameof(Revive), 3f);
    }

    private void Revive()
    {
        _body.UnRagdoll();
        _body.Play("IsMoving", false);
        OnDeathCollider(false);
        health.Value = maxHealth;
        isAlive.Value = true;
    }

    private void OnDeathCollider(bool isDead)
    {
        _collider.enabled = !isDead;
    }
}
