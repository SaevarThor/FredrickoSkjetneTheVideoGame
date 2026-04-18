using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button StartButton; 

    private void Start()
    {
        StartButton.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        // Load the first level or main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
