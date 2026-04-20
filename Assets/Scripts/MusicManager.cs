using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{ 
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] clips; 
    private int clipIndex;

    private float timer; 
    private float clipLifetime; 

    private bool paused = false; 


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
        if (paused) return;

        timer += Time.deltaTime; 

        if (timer >= clipLifetime)
        {
            SetNewClip();
        }
    }


    public void Pause()
    {
        source.Pause();
        paused = true; 
    }

    public void Unpause()
    {
        source.Play();
        paused = false; 
    }
}
