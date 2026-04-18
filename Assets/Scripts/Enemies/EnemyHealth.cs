using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{

    public float MaxHealth = 100f;
    public float CurrentHealth { get; private set; }

    public float Score = 20f; 

    private bool isDead = false; 

    [SerializeField] private GameObject bloodExplosion;

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return; 
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
        isDead = true; 
        Instantiate(bloodExplosion, transform.position, Quaternion.identity);

        GameObject levelManager = GameObject.FindGameObjectWithTag("LevelManager");
        var levelManagerScript = levelManager.GetComponent<LevelManager>();
        levelManagerScript.EnemyKilled(Score);

        Destroy(this.gameObject);
    }
}
