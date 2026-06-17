using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public abstract class BaseCharacter : NetworkBehaviour 
{
    [SerializeField] protected NetworkVariable<float> currentHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] protected NetworkVariable<float> maxHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public event Action OnCharacterDeath;




    public virtual void TakeDamage(float damage)
    {
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0)
        {
            Die();
        }

        Debug.Log("Client " + OwnerClientId + " has taken damage");
    }

    public virtual void Die()
    {
        OnCharacterDeath.Invoke();

        Destroy(gameObject);
    }

    public virtual void Heal(float amount)
    {
        currentHealth.Value += amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth.Value);
    }
}
