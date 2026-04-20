using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public float LevelTimer;
    public int EnemiesKilled;
    public int EnemiesInLevel; 
    public int CurrentLevelIndex = 0;


    private float ScoreLevel = 800; 
    private float scoreFromEnemies =  0; 
    public TMP_Text LevelStatsText; 
    public GameObject LevelCompleteUI;

    public float secretScore = 0; 

    private bool _levelEnded = false;

    void Update()
    {
        if (!_levelEnded)
        {
            LevelTimer += Time.deltaTime;
        }
    }


    public void EnemyKilled(float score)
    {
        EnemiesKilled++;
        ScoreLevel += score;

        scoreFromEnemies += score; 
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        LevelTimer = 0f; 
        SceneManager.LoadScene(CurrentLevelIndex);
    }

    public void PlayNextLevel()
    {
        Time.timeScale = 1f;
        LevelTimer = 0f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }  

    public void FoundSecret()
    {
        secretScore += 200;
    } 

    public void EndLevel()
    {
        Time.timeScale = 0f;
        _levelEnded = true;

        LevelTimer = Mathf.RoundToInt(LevelTimer); 
        ScoreLevel-= LevelTimer; 

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LevelStatsText.text = $" Time: {LevelTimer:F2}\n Enemies Killed: {EnemiesKilled}\n \n <size=140%>Score: {ScoreLevel}</size> \n From Killing: +{scoreFromEnemies} \n From Time: +{800 - LevelTimer}\n From Secrets: +{secretScore}";
        LevelCompleteUI.SetActive(true);

        var levelEntry = new LevelEntry
        {
            LevelIndex = CurrentLevelIndex,
            TimeTaken = LevelTimer,
            EnemiesKilled = EnemiesKilled,
            Score = ScoreLevel
        };

        var gameManager = ReferenceManager.Instance.gameManager;
        var oldEntry = gameManager.GetLevelEntry(CurrentLevelIndex);
        gameManager.Score += Mathf.RoundToInt(ScoreLevel); 

        if (oldEntry == null || levelEntry.Score > oldEntry.Score)
        {
            gameManager.AddLevelEntry(levelEntry);
        }



    
    }



}
