using UnityEngine.UI; 
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DieVisuals : MonoBehaviour
{
    public float alphaLevel; 
    private Color backgroundColor;
    public Image deathPanelBackground; 
    public TMP_Text deadText; 
    [SerializeField] private GameObject DeathCamera; 
    [SerializeField]private bool isDead; 
    [SerializeField] private Button restartButton; 


    private void Start()
    {
        backgroundColor = deathPanelBackground.color; 
        restartButton.onClick.AddListener(() => Restart()); 
    }

    public void StartDeath(Vector3 loc)
    {
        isDead = true;
        var trans = ReferenceManager.Instance.PlayerTransform; 
        Instantiate(DeathCamera, loc, trans.GetComponentInChildren<Camera>().transform.rotation); 
        trans.gameObject.SetActive(false); 
        restartButton.gameObject.SetActive(true); 

        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true; 

        deadText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (isDead)
        {
            Color tempColor = backgroundColor;
            deathPanelBackground.color = new Color(tempColor.r, tempColor.g, tempColor.b, alphaLevel);
            alphaLevel += Time.deltaTime;

            deadText.fontSize = Mathf.Lerp(deadText.fontSize, 800, 0.1f * Time.deltaTime); 
        }
    }


    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
    }
}
