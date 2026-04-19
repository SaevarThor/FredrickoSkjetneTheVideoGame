using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager Instance { get; private set; }

    public Camera PlayerCamera;
    public Transform PlayerTransform;
    public GameManager gameManager;

    public MessagingManager mManager;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    
}
