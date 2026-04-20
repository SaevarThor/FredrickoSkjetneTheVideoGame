using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossLevel : MonoBehaviour
{
    public GameObject Boss;
    private bool gameEnd = false; 

    private float timer = 10; 

    public GameObject[] Pillars; 
    public GameObject[] secretPillars;
    public GameObject[] secretPillars2; 

    public EnemyHealth bossHealth;
    public GameObject phase2Enemies; 
    public GameObject phase3Enemies; 

    private void Start()
    {
        bossHealth.MakeInvulnerable();
    }

    private void Update()
    {
        if (Boss == null && !gameEnd )
        {
            StartCoroutine(EndGame());
            gameEnd = true; 
        }

        if (AllPillarsGone() && !bossHealth.CanTakeDamage())
        {
            bossHealth.MakeVulnerable();
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(timer); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
    }


    private bool AllPillarsGone()
    {
        bool allPillarsGone = true; 
        foreach(var p in Pillars)
        {
            if (p != null)
            {
                allPillarsGone = false; 
            }
        }

        return allPillarsGone; 
    }
    public void Phase2Pillars()
    {
        Pillars = secretPillars; 
        bossHealth.MakeInvulnerable();

        foreach(var p in Pillars)
        {
            p.SetActive(true); 
        }

        phase2Enemies.SetActive(true);
    }

        public void Phase3Pillars()
    {
        Pillars = secretPillars2; 
        bossHealth.MakeInvulnerable();

        foreach(var p in Pillars)
        {
            p.SetActive(true); 
        }

        phase3Enemies.SetActive(true);
    }
}
