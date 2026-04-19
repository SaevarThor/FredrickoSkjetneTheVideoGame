using UnityEngine;

public class ShieldPickUp : MonoBehaviour
{
    [SerializeField] private float shieldAmount = 25f;
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                if (playerHealth.GainShield(shieldAmount))
                {
                    other.GetComponent<AudioSource>().PlayOneShot(pickupSound);
                    Destroy(gameObject);
                }
            }
        }
    }
}
