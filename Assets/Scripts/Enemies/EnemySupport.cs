using UnityEngine;
using System.Collections;


public class EnemySupport: MonoBehaviour
{
	private EnemyAIFlyingSupport Ai;
    public ParticleSystem particle;
    private Transform _currentAlly;
    
    private void Awake()
    {
        Ai = GetComponent<EnemyAIFlyingSupport>();
        _currentAlly = Ai._currentAlly;
        
    }

    // Update is called once per frame
    void Update()
	{
        _currentAlly = Ai._currentAlly;
        if (Ai._currentAlly != null)
        {
            var enemy = Ai._currentAlly.GetComponent<EnemyHealth>();
            switch (Ai._flyState)
            {
                case EnemyAIFlyingSupport.FlyState.Protecting:

                    if (enemy != null)
                    {
                        enemy.MakeInvulnerable();
                    }
                    if (!particle.isPlaying)
                    {
                        particle.Play();
                    }
                    break;

                default:
                    particle.Stop();
                    if (enemy != null)
                    {
                        enemy.MakeVulnerable();
                    }
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        if (_currentAlly != null)
        {
            var enemy = Ai._currentAlly.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.MakeVulnerable();
            }
        }
    }
}
