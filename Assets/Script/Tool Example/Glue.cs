using UnityEngine;

public class Glue : MonoBehaviour 
{
    [Header("Glue Settings")]
    public float glueRadius = 0.3f;        // รัศมีการทำงานของกาว
    public float repairStrength = 0.2f;    // ความแรงในการซ่อมแซม (0-1)
    public float glueAmount = 100f;        // ปริมาณกาวที่มี
    public float glueUsagePerRepair = 5f;  // ปริมาณกาวที่ใช้ต่อครั้ง
    
    [Header("Effects")]
    public ParticleSystem glueEffect;      // เอฟเฟกต์กาว
    public AudioClip glueSound;           // เสียงกาว
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void UseGlue(Vector3 position)
    {
        // ถ้ากาวหมด ไม่สามารถใช้งานได้
        if (glueAmount <= 0) return;

        // หาฟอสซิลในรัศมี
        Collider[] hits = Physics.OverlapSphere(position, glueRadius);
        bool repaired = false;
        
        foreach (var hit in hits)
        {
            Fossil fossil = hit.GetComponent<Fossil>();
            if (fossil != null && fossil.GetCurrentState() == Fossil.FossilState.Damaged)
            {
                // ใช้กาวซ่อมฟอสซิล
                fossil.Repair();
                repaired = true;

                // ลดปริมาณกาว
                glueAmount -= glueUsagePerRepair;

                // เล่นเอฟเฟกต์
                if (glueEffect != null)
                {
                    var effect = Instantiate(glueEffect, hit.transform.position, Quaternion.identity);
                    effect.transform.rotation = Quaternion.LookRotation(Vector3.up);
                    Destroy(effect.gameObject, 2f);
                }
            }
        }
        
        // เล่นเสียงถ้าซ่อมสำเร็จ
        if (repaired && glueSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(glueSound);
        }
    }

    // สำหรับแสดง Gizmos ในหน้า Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawSphere(transform.position, glueRadius);
    }

    // สำหรับเช็คว่ากาวเหลือพอใช้หรือไม่
    public bool HasGlue()
    {
        return glueAmount > 0;
    }

    // สำหรับเติมกาว
    public void RefillGlue(float amount)
    {
        glueAmount = Mathf.Min(glueAmount + amount, 100f);
    }
}
