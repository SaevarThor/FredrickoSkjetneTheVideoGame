using UnityEngine;

public class HealthPickUp : MonoBehaviour
{
   [SerializeField] private float healAmount = 25f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.Heal(healAmount))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
