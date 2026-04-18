using UnityEngine;

public class ShieldPickUp : MonoBehaviour
{
    [SerializeField] private float shieldAmount = 25f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.GainShield(shieldAmount))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
