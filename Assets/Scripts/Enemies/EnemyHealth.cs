using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{

    public float MaxHealth = 100f;
    public float CurrentHealth { get; private set; }

    public float Score = 20f; 

    private bool isDead = false;

    private bool isInvulnerable = false;

    [SerializeField] private GameObject bloodExplosion;
    [SerializeField] private GameObject shieldVisuals;

    private void Awake()
    {
        if (shieldVisuals != null) {
            shieldVisuals.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public bool CanTakeDamage()
    {
        return !isInvulnerable; 
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        if (isInvulnerable) return;
        // Implementation for taking damage
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    
    public void MakeInvulnerable()
    {
        isInvulnerable = true;
        shieldVisuals.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void MakeVulnerable()
    {
        shieldVisuals.GetComponent<SpriteRenderer>().enabled = false;
        isInvulnerable = false;
    }




    private void Die()
    {
        isDead = true;
        if (bloodExplosion != null)
        {
            Instantiate(bloodExplosion, transform.position, Quaternion.identity);
        }

        GameObject levelManager = GameObject.FindGameObjectWithTag("LevelManager");
        var levelManagerScript = levelManager.GetComponent<LevelManager>();
        levelManagerScript.EnemyKilled(Score);

        Destroy(this.gameObject);
    }
}
