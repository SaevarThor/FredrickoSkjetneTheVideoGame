using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float Timer; 

    void Update()
    {
        Timer += Time.deltaTime;
    }
}
