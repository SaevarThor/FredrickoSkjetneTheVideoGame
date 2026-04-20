using TMPro;
using UnityEngine;

public class TimerThroughGame : MonoBehaviour
{
    public TMP_Text text;


    private void Start()
    {
        var m = ReferenceManager.Instance.gameManager;

        float gameTime = m.GameTime;
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);

        text.text = $"You beat the game in {minutes:00}:{seconds:00}";
    }
}
