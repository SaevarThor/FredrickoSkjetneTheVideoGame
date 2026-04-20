using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [TextArea(5, 10)] public string[] introTexts = new string[] { "Welcome to Shotgun Zaddy!", "Prepare for an epic adventure..." };
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.05f;
    public float pauseBetweenTexts = 1f;

    public float waitBeforeLoadingNextScene = 4f;

    public bool isFirst = false;

    public Button[] skipArray; 
    private int skipIndex = 0; 

    void Start()
    {
        StartCoroutine(TypeText());

        if (!isFirst)
            ReferenceManager.Instance.GetComponentInChildren<MusicManager>().Pause();


        skipArray[skipIndex].onClick.AddListener(() => Skip()); 
    }


    public void Skip()
    {
        skipArray[skipIndex].gameObject.SetActive(false);
        skipIndex++;

        if (skipArray.Length == skipIndex + 1)
        {
            skipArray[skipIndex].onClick.AddListener(() => LoadNextLevel()); 
            skipArray[skipIndex].gameObject.SetActive(true);
            return;
        }

        skipArray[skipIndex].gameObject.SetActive(true);
        skipArray[skipIndex].onClick.AddListener(() => Skip()); 
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

    private void LoadNextLevel()
    {
        if (!isFirst)
            ReferenceManager.Instance.GetComponentInChildren<MusicManager>().Unpause();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
