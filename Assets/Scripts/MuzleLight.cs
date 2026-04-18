using UnityEngine;

public class MuzleLight : MonoBehaviour
{
    [SerializeField] private float lightDuration = 0.1f;

    private Light _light;
    private float _timer;

    private void Awake()
    {
        _light = GetComponent<Light>();
        if (_light == null)
        {
            Debug.LogError("MuzleLight requires a Light component on the same GameObject.");
            enabled = false;
            return;
        }
        _light.enabled = false;
    }

    private void Update()
    {
        if (_light.enabled)
        {
            _timer += Time.deltaTime;
            if (_timer >= lightDuration)
            {
                _light.enabled = false;
                _timer = 0f;
            }
        }
    }

    public void Flash()
    {
        _light.enabled = true;
        _timer = 0f;
    }
}
