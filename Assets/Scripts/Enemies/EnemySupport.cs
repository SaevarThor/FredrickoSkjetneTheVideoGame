using UnityEngine;
using System.Collections;


public class EnemySupport: MonoBehaviour
{
	private EnemyAIFlyingSupport Ai;
    public ParticleSystem particle;
    
    private void Awake()
    {
        Ai = GetComponent<EnemyAIFlyingSupport>();
        
    }

    // Update is called once per frame
    void Update()
	{
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
        if (Ai._currentAlly != null)
        {
            var enemy = Ai._currentAlly.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.MakeVulnerable();
            }
        }
    }
}
