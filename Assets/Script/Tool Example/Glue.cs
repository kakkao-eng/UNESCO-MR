using UnityEngine;

public class Glue : MonoBehaviour 
{
    [Header("Glue Settings")]
    public float glueRadius = 0.3f;        // รัศมีของกาว
    public float repairStrength = 0.2f;    // ความแรงในการซ่อม
    public float glueAmount = 100f;        // ปริมาณกาวที่มี
    public float glueUsagePerRepair = 5f;  // กาวที่ใช้ต่อครั้ง

    [Header("Effects")]
    public ParticleSystem glueEffect;      // เอฟเฟกต์กาวเวลาใช้งาน

    public void UseGlue(Vector3 position)
    {
        if (glueAmount <= 0) return; // ถ้ากาวหมด ไม่ทำงาน

        Collider[] hits = Physics.OverlapSphere(position, glueRadius);
        bool repaired = false;

        foreach (var hit in hits)
        {
            Fossil fossil = hit.GetComponent<Fossil>();
            if (fossil != null && fossil.GetCurrentState() == Fossil.FossilState.Damaged)
            {
                fossil.Repair(); // ซ่อมฟอสซิล
                repaired = true;

                glueAmount -= glueUsagePerRepair; // ลดจำนวนกาว

                if (glueEffect != null)
                {
                    var effect = Instantiate(glueEffect, hit.transform.position, Quaternion.identity);
                    effect.transform.rotation = Quaternion.LookRotation(Vector3.up);
                    Destroy(effect.gameObject, 2f);
                }
            }
        }

        // เล่นเสียงถ้า Repair สำเร็จ
        if (repaired) SoundManager.PlaySound(SoundType.Glue);
    }

    // Gizmos สำหรับ Debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawSphere(transform.position, glueRadius);
    }

    public bool HasGlue()
    {
        return glueAmount > 0; // เช็คว่ามีกาวเหลือหรือไม่
    }

    public void RefillGlue(float amount)
    {
        glueAmount = Mathf.Min(glueAmount + amount, 100f); // เติมกาวแต่ไม่เกิน 100
    }
}
