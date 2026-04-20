using UnityEngine;

public class SecretTrigger : MonoBehaviour
{
    private AudioSource secretSource;

    private bool isdone = false; 

    void Start()
    {
        secretSource = GetComponent<AudioSource>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (isdone) return;

        if (other.CompareTag("Player"))
        {
            secretSource.Play();
            
            GameObject levelManager = GameObject.FindGameObjectWithTag("LevelManager");
            var levelManagerScript = levelManager.GetComponent<LevelManager>();
            levelManagerScript.FoundSecret();

            isdone = true; 
            
        }      
    }
}
