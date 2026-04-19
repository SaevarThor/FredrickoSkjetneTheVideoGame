using UnityEngine;

public class checkpoint : MonoBehaviour
{
   public DeathTrigger deathTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            deathTrigger.Position = transform.position; 
            Destroy(this.gameObject);
        }
    }
}
