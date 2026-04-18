using UnityEngine;

public class EndTheLevel : MonoBehaviour
{

    [SerializeField] private LevelManager levelManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            levelManager.EndLevel();
        }
    }
}
