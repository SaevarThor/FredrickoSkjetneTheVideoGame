using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrumbus : MonoBehaviour
{
    private AudioSource source; 
    [SerializeField] private AudioClip[] scrumbusClips;

    // Update is called once per frame
    void Start()
    {
        source = GetComponent<AudioSource>();
        StartCoroutine(WaitAndPlay());
    }


    private IEnumerator WaitAndPlay()
    {
        while(true)
        {
            yield return new WaitForSeconds(20);
            source.clip = scrumbusClips[Random.Range(0, scrumbusClips.Length)];
            source.Play();
        }
    }
}
