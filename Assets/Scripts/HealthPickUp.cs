using UnityEngine;

public class HealthPickUp : MonoBehaviour
{
   [SerializeField] private float healAmount = 25f;
   [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                if (playerHealth.Heal(healAmount))
                {
                    other.GetComponent<AudioSource>().PlayOneShot(pickupSound);
                    Destroy(gameObject);
                }
            }
        }
    }
}
