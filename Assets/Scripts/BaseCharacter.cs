using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public abstract class BaseCharacter : NetworkBehaviour 
{
    public float currentHealth;
    public float maxHealth;
    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }

    public virtual void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}
