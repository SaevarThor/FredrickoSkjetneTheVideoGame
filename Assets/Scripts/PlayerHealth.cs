using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float MaxShield = 100f;
    public float playerShield = 50f; 
    public float MaxHealth = 200f;
    public float CurrentHealth;

    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text shieldText;

    [SerializeField] private AudioSource hitSource;
    [SerializeField] private AudioClip[] hitClips; 
    [SerializeField] private AudioClip[] shieldHitClips;


    private void Start()
    {
        CurrentHealth = 100;
    }

    public bool CanTakeDamage()
    {
        return true; 
    }

    public void TakeDamage(float amount)
    {

        if (playerShield > 0)
        {
            float shieldDamage = Mathf.Min(playerShield, amount);
            playerShield -= shieldDamage;
            amount -= shieldDamage;
        }

        CurrentHealth -= amount;

        if (amount > 0 && hitSource != null && hitClips.Length > 0)
        {
            hitSource.PlayOneShot(hitClips[Random.Range(0, hitClips.Length)]);
            CameraShake.Instance.Shake(10);
        }
        if (amount == 0 && hitSource != null && shieldHitClips.Length > 0)
        {
            hitSource.PlayOneShot(shieldHitClips[Random.Range(0, shieldHitClips.Length)]);
        }

        UpdateUI();

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }

    }

    public bool Heal(float amount)
    {
        if (CurrentHealth == MaxHealth) return false;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        UpdateUI();
        return true;
    }

    public bool GainShield(float amount)
    {
        if (playerShield == MaxShield) return false;

        playerShield += amount;
        playerShield = Mathf.Clamp(playerShield, 0, MaxShield);
        UpdateUI();
        return true;
    }   

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"{CurrentHealth}";

        if (shieldText != null)
            shieldText.text = $"{playerShield}";
    }

    private void Die()
    {
        Debug.Log("Player died!");
        GameObject.Find("DeathPanel").GetComponent<DieVisuals>().StartDeath(transform.position);
        // Implement player death behavior (e.g., respawn, game over screen)
    }
}
