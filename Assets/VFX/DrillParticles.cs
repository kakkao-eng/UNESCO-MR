using UnityEngine;

public class DrillParticles : MonoBehaviour
{
    private ParticleSystem particles;

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        if (particles == null)
        {
            Debug.LogError("Missing ParticleSystem component!", this);
        }
    }

    public void SetParticleColor(Color color)
    {
        if (particles != null)
        {
            var main = particles.main;
            main.startColor = color;
        }
    }
}