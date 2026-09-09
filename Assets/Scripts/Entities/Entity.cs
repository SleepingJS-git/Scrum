using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Abstract class of all Entities. It contains the onNetworkSpawn, health, onHit, and onDeath logic.
/// </summary>
public abstract class Entity : NetworkBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private UnityEvent onDeathEffects;
    private event Action OnDeath; 
    private OnHitData lastHitData;
    public NetworkVariable<int> health = new();
    public NetworkVariable<bool> isAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    /// <summary>
    /// When the object is spawned on the Network, set its health to its max
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
        }
    }

    /// <summary>
    /// When the entity is hit, subtract health, then check if it is dead.
    /// </summary>
    /// <param name="damage"></param>
    public virtual void OnHit(OnHitData onHitData)
    {
        lastHitData = onHitData;

        OnHitServerRpc(onHitData.damage);
    }
    
    /// <summary>
    /// Subscribe events for OnDeath
    /// </summary>
    public virtual void AddDeathEvent(Action action)
    {
        OnDeath += action;
    }

    /// <summary>
    /// Registers the hit onto the server.
    /// </summary>
    /// <param name="damage"></param>
    [ServerRpc]
    private void OnHitServerRpc(int damage)
    {
        health.Value -= damage;

        if (health.Value <= 0)
        {
            OnDeath?.Invoke();

            OnDeathEffectsClientRpc();
        }
    }

    /// <summary>
    /// This plays death effects that will display differently on all clients.
    /// </summary>
    [ClientRpc]
    private void OnDeathEffectsClientRpc()
    {
        onDeathEffects?.Invoke();
    }

}