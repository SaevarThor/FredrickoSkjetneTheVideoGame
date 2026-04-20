using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossLevel : MonoBehaviour
{
    public GameObject Boss;
    private bool gameEnd = false; 

    private float timer = 10; 

    private void Update()
    {
        if (Boss == null && !gameEnd )
        {
            StartCoroutine(EndGame());
            gameEnd = true; 
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(timer); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
    }
}
