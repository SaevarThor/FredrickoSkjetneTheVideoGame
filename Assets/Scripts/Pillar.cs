using UnityEngine;

public class Pillar : MonoBehaviour
{
    public Transform Boss; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SupportBeam>().SetTarget(Boss); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
