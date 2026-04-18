using UnityEngine;

public class AutoDestroy : MonoBehaviour
{

    [SerializeField] private float destroyDelay = 2f;

    void Start()
    {
        Destroy(gameObject, destroyDelay);
    }

   
}
