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
        base.OnHit(onHitData);
        _body.onHitData = onHitData;
        // audioSource. play hurt noise
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
