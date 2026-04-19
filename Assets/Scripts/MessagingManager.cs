
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessagingManager : MonoBehaviour
{
   public GameObject TextBackground;
   public TMP_Text MessageText;
   public TMP_Text SideText; 

   public float TimeBetweenFades = 3;
   
   private bool _isShowing;
   private bool _doneFading;

   private float _tElapsed;
   private float _alphaLevels;

   public float gbValues = 255;

    private void Start()
    {
        ReferenceManager.Instance.mManager = this; 
    }

    void Update()
   {
      if (!_isShowing)
         return;

      if (_isShowing && !_doneFading)
      {
         FadeIn();
      }

      if (_doneFading)
      {
         _tElapsed += Time.deltaTime;

         if (_tElapsed >= TimeBetweenFades)
         {
            FadeOut();
         }
      }
   }

   public void UpdateObjective(string newOBj, string sideObj = null)
   {
      SideText.text = "";
      MessageText.text = newOBj;
      if (!string.IsNullOrEmpty(sideObj))
      {
         SideText.text = sideObj;
      }

      _isShowing = true;
      _doneFading = false;
      _tElapsed = 0;
   }

   private void FadeIn()
   {

      Color tempColor = TextBackground.GetComponent<Image>().color;
      TextBackground.GetComponent<Image>().color = new Color(tempColor.r, tempColor.g, tempColor.b, _alphaLevels);
      MessageText.color = new Color(12, 12, 12, _alphaLevels);
      SideText.color =   new Color(12, gbValues, gbValues, _alphaLevels);

      _alphaLevels += Time.deltaTime;

      if (_alphaLevels >= 1)
      {
         _doneFading = true;
      }
   }

   private void FadeOut()
   {
      Color tempColor = TextBackground.GetComponent<Image>().color;
      TextBackground.GetComponent<Image>().color = new Color(tempColor.r, tempColor.g, tempColor.b, _alphaLevels);
      MessageText.color = new Color(12, 12, 12, _alphaLevels);
      SideText.color =   new Color(12, gbValues, gbValues, _alphaLevels);

      _alphaLevels -= Time.deltaTime;

      if (_alphaLevels <= 0)
      {
         SideText.color = new Color(12, 255, 255, 0);
         MessageText.text = "";
         _tElapsed = 0;
         _isShowing = false;
      }
   }
}
