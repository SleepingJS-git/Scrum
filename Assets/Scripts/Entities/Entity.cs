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
    public NetworkVariable<int> health = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<bool> isAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
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
    /// [Called by Server] (This method is still being ran on the server)
    /// When the entity is hit, subtract health, then check if it is dead.
    /// </summary>
    /// <param name="damage"></param>
    public virtual void OnHit(OnHitData onHitData)
    {
        lastHitData = onHitData;

        health.Value -= onHitData.damage;

        if (health.Value <= 0)
        {
            isAlive.Value = false;
            
            OnDeath?.Invoke();

            OnDeathEffectsClientRpc();
        }

        Debug.Log($"{gameObject.name} was damaged by {onHitData.attacker.name} ({onHitData.damage} - {onHitData.damageType})");
    }

    /// <summary>
    /// Subscribe events for OnDeath
    /// </summary>
    public virtual void AddDeathEvent(Action action)
    {
        OnDeath += action;
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