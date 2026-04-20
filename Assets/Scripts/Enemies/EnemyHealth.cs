using System.Collections;
using TMPro;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{

    public float MaxHealth = 100f;
    public float CurrentHealth { get; set; }
    public float Score = 20f; 
    private bool isDead = false;
    private bool isInvulnerable = false;

    [SerializeField] private Sprite hitSprite; 
    private Sprite normalSprite; 

    [SerializeField] private SpriteRenderer spriteRenderer; 

    [SerializeField] private GameObject bloodExplosion;
    [SerializeField] private GameObject shieldVisuals;

    public bool isBoss; 

    private void Awake()
    {
        if (shieldVisuals != null) {
            shieldVisuals.GetComponent<SpriteRenderer>().enabled = false;
        }

        if (spriteRenderer!= null)
            normalSprite = spriteRenderer.sprite; 
    }
    private void Start()
    {
        var run = ReferenceManager.Instance.RoundNumber; 

        if (run > 1)
        {
            var additionaHealth = 1;

            for (int i = 1; i < run; i++)
            {
                additionaHealth *= 10;  
            }

            MaxHealth *= additionaHealth; 
        }
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

        if (hitSprite != null)
        {
            spriteRenderer.sprite = hitSprite; 

            StopCoroutine(BackToDefault()); 
            StartCoroutine(BackToDefault()); 
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


    private IEnumerator BackToDefault()
    {
        yield return new WaitForSeconds(0.5f); 
        spriteRenderer.sprite = normalSprite; 
    }



    private void Die()
    {
        if (isBoss) return; 
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
