using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public float LevelTimer;
    public int EnemiesKilled;
    public int EnemiesInLevel; 
    public int CurrentLevelIndex = 0;


    public float ScoreLevel = 400; 
    public TMP_Text LevelStatsText; 
    public GameObject LevelCompleteUI;

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
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        LevelTimer = 0f; 
        UnityEngine.SceneManagement.SceneManager.LoadScene(CurrentLevelIndex);
    }

    public void PlayNextLevel()
    {
        Time.timeScale = 1f;
        LevelTimer = 0f; 
        UnityEngine.SceneManagement.SceneManager.LoadScene(CurrentLevelIndex + 1);
    }   

    public void EndLevel()
    {
        Time.timeScale = 0f;
        _levelEnded = true;
        ScoreLevel-= LevelTimer; 

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LevelStatsText.text = $" Time: {LevelTimer:F2}\n Enemies Killed: {EnemiesKilled}/{EnemiesInLevel} \n Score: {ScoreLevel}";
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

        if (oldEntry == null || levelEntry.Score > oldEntry.Score)
        {
            gameManager.AddLevelEntry(levelEntry);
        }



    
    }



}
