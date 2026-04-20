using UnityEngine;

public class DamagePlayerTrigger : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 12f;    // How far the player gets launched
    [SerializeField] private float knockbackUpward = 5f;    // Upward pop on hit
    [SerializeField] private float knockbackDuration = 0.3f; // How long the push lasts

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Damage
        other.GetComponent<IDamageable>()?.TakeDamage(damage);

        // Knockback — direction is away from this object's center
        CharacterController cc = other.GetComponent<CharacterController>();
        PlayerMovement pm      = other.GetComponent<PlayerMovement>();
        CameraShake cs = other.GetComponentInChildren<CameraShake>(); 

        if (cc != null && pm != null)
        {
            Vector3 pushDir = (other.transform.position - transform.position).normalized;
            pushDir.y = 0f; // Zero out vertical before adding the upward pop
            Vector3 knockback = pushDir * knockbackForce + Vector3.up * knockbackUpward;

            pm.ApplyKnockback(knockback, knockbackDuration);
        }

        cs.Shake(10, 20);
    }
}