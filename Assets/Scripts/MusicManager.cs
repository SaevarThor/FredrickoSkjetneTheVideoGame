using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{ 
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] clips; 
    private int clipIndex;

    private float timer; 
    private float clipLifetime; 


    public void SetNewClip()
    {
        var clip = clips[clipIndex]; 
        clipLifetime = clip.length; 
        timer = 0; 

        clipIndex++; 
        if (clipIndex > clips.Length)
        {
            clipIndex = 0;
        }

        source.clip = clip; 
        source.Play();
    }

    private void Start()
    {
        SetNewClip();
    }

    private void Update()
    {
        timer += Time.deltaTime; 

        if (timer >= clipLifetime)
        {
            SetNewClip();
        }
    }
}
