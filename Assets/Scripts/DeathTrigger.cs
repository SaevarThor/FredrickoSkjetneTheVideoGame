using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTrigger : MonoBehaviour
{
    public Vector3 Position;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player  = ReferenceManager.Instance.PlayerTransform.GetComponent<PlayerMovement>();
            player.TeleportPlayer(Position);
        }
    }
}
