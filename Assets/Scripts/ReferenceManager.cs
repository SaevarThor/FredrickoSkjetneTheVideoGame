using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager Instance { get; private set; }

    public Camera PlayerCamera;
    public Transform PlayerTransform;
    public GameManager gameManager;

    public MessagingManager mManager;

    public int RoundNumber = 1;


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
