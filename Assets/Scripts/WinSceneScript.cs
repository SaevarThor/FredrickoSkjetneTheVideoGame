using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinSceneScript : MonoBehaviour
{
   [SerializeField] private string m = "Start New Game";
   [SerializeField] private TMP_Text buttonText;


    void Start()
    {
        var run = ReferenceManager.Instance.RoundNumber; 

        for(var i = 0; i < run; i++)
        {
            m+= "+"; 
        }

        buttonText.text = m; 

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 
    }


    public void StartNewGame()
    {
        ReferenceManager.Instance.RoundNumber++; 
        SceneManager.LoadScene(1); 
    }
}
