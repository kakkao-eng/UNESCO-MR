using UnityEngine;

public class Brush : MonoBehaviour 
{
    [Header("Brush Settings")]
    public float brushRadius = 0.5f;        // รัศมีการทำงานของแปรง
    public float brushStrength = 0.3f;      // ความแรงของแปรง (อาจใช้ควบคุมความเสียหาย/ลดค่า)

    [Header("Effects")]
    public ParticleSystem dustEffect;        // เอฟเฟกต์ฝุ่นที่ปล่อยออกมาเวลาแปรงดิน

    public void UseBrush(Vector3 position)
    {
        // หาบล็อกดินรอบ ๆ จุดที่ผู้เล่นใช้แปรง
        Collider[] hits = Physics.OverlapSphere(position, brushRadius);
        bool hitSomething = false;

        foreach (var hit in hits)
        {
            // เช็คว่าชน SoilBlock หรือไม่
            SoilBlock soil = hit.GetComponent<SoilBlock>();
            if (soil != null && soil.soilType == SoilType.NearFossil) // ปัดเฉพาะดินใกล้ Fossil
            {
                soil.Brush(); // เรียกฟังก์ชัน Brush() ของดิน
                hitSomething = true;

                // สร้างเอฟเฟกต์ฝุ่น
                if (dustEffect != null)
                {
                    var dust = Instantiate(dustEffect, soil.transform.position, Quaternion.identity);
                    Destroy(dust.gameObject, 2f); // ลบเอฟเฟกต์ภายใน 2 วิ
                }
            }
        }

        // เล่นเสียงถ้าเจอดิน
        if (hitSomething)
        {
            SoundManager.PlaySound(SoundType.Brush);
        }
    }

    // Gizmo สำหรับ Debug แสดงรัศมีของแปรงใน Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, brushRadius);
    }
}