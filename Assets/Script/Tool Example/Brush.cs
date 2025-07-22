using UnityEngine;

public class Brush : MonoBehaviour 
{
    [Header("Brush Settings")]
    public float brushRadius = 0.5f;        // รัศมีการทำงานของแปรง
    public float brushStrength = 0.3f;      // ความแรงในการปัด (0-1)
    
    [Header("Effects")]
    public ParticleSystem dustEffect;        // เอฟเฟกต์ฝุ่น
    public AudioClip brushSound;            // เสียงแปรง
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void UseBrush(Vector3 position)
    {
        // หาบล็อกดินในรัศมี
        Collider[] hits = Physics.OverlapSphere(position, brushRadius);
        bool hitSomething = false;
        
        foreach (var hit in hits)
        {
            SoilBlock soil = hit.GetComponent<SoilBlock>();
            if (soil != null && soil.soilType == SoilType.NearFossil)
            {
                // ใช้แปรงกับดิน
                soil.Brush();
                hitSomething = true;

                // เล่นเอฟเฟกต์
                if (dustEffect != null)
                {
                    var dust = Instantiate(dustEffect, soil.transform.position, Quaternion.identity);
                    Destroy(dust.gameObject, 2f);
                }
            }
        }
        
        // เล่นเสียงถ้าแปรงโดนดิน
        if (hitSomething && brushSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(brushSound);
        }
    }

    // สำหรับแสดง Gizmos ในหน้า Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, brushRadius);
    }
}