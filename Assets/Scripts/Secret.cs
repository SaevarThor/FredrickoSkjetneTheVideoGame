using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Secret : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    private AudioSource _audioSource;
    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        other.GetComponent<AudioSource>().PlayOneShot(clip);
        GetComponent<BoxCollider>().enabled = false;
    }
}
