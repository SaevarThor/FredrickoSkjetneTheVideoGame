using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{

    public float MaxHealth { get; private set; } = 100f;
    public float CurrentHealth { get; private set; }

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        // Implementation for taking damage
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }


    private void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(this.gameObject);
    }
}
