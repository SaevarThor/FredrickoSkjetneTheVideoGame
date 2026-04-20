using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [TextArea(5, 10)] public string[] introTexts = new string[] { "Welcome to Shotgun Zaddy!", "Prepare for an epic adventure..." };
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.05f;
    public float pauseBetweenTexts = 1f;

    public float waitBeforeLoadingNextScene = 4f;

    public bool isFirst = false;

    void Start()
    {
        StartCoroutine(TypeText());

        if (!isFirst)
            ReferenceManager.Instance.GetComponentInChildren<MusicManager>().Pause();
    }

    IEnumerator TypeText()
    {


        textComponent.text = "";
        foreach (string text in introTexts)
        {
            foreach (char letter in text.ToCharArray())
            {
                if (letter.ToString() == "<")
                {
                    textComponent.text += "<b>"; 
                    continue;
                }
                if (letter.ToString() == ">")
                {
                    textComponent.text += "</b>"; 
                    continue;
                }

                
                textComponent.text += letter;
                yield return new WaitForSeconds(typingSpeed);

            }
            // Pause after each text
            yield return new WaitForSeconds(pauseBetweenTexts);
            // Add double enter (two newlines)
            textComponent.text += "\n\n";
        }

        yield return new WaitForSeconds(waitBeforeLoadingNextScene);
        // All texts done, load next scene
        if (!isFirst)
            ReferenceManager.Instance.GetComponentInChildren<MusicManager>().Unpause();
            
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
