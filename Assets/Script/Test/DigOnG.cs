using UnityEngine;
using UnityEngine.InputSystem; // 👈 สำหรับ New Input System

public class DigOnG_InputSystem : MonoBehaviour
{
    [Header("Digging Settings")]
    public float range = 0.05f;      // รัศมีการขุด
    public float damage = 0.5f;      // ความเสียหาย
    public LayerMask digMask;        // เลือก layer ที่จะขุดได้
    
    [Header("Tool Settings")]
    public ToolType toolType = ToolType.HammerChisel; // เพิ่มประเภทเครื่องมือ
    
    [Header("Feedback")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.green;

    void Update()
    {
        // กดปุ่ม G เพื่อขุด
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            PerformDig();
        }
    }

    void PerformDig()
    {
        // ตรวจจับบล็อกในรัศมีที่กำหนด
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range, digMask);
        int objectsAffected = 0;

        foreach (Collider col in hitColliders)
        {
            bool wasAffected = false;

            // เช็ค Fossil ก่อน
            Fossil fossil = col.GetComponent<Fossil>();
            if (fossil != null)
            {
                fossil.TakeDamage(damage, toolType);
                wasAffected = true;
                Debug.Log($"Hit Fossil with {toolType}, Damage: {damage}");
            }
            else // ถ้าไม่ใช่ Fossil ให้เช็ค SoilBlock
            {
                SoilBlock block = col.GetComponent<SoilBlock>();
                if (block != null)
                {
                    // ความเสียหายจะแตกต่างกันตามประเภทของบล็อก
                    if (block.soilType == SoilType.Fossil)
                    {
                        Debug.LogWarning("Found fossil! Be careful!");
                        block.TakeDamage(damage * 0.5f);  // ฟอสซิลเสียหาย 50%
                    }
                    else if (block.soilType == SoilType.NearFossil)
                    {
                        Debug.Log("Near fossil! Use more precise tools!");
                        block.TakeDamage(damage * 0.75f); // ใกล้ฟอสซิลเสียหาย 75%
                    }
                    else
                    {
                        block.TakeDamage(damage);         // บล็อกปกติเสียหาย 100%
                    }
                    wasAffected = true;
                }
            }

            if (wasAffected)
            {
                objectsAffected++;
                Debug.Log($"Hit object at {col.transform.position}");
            }
        }

        if (objectsAffected > 0)
        {
            Debug.Log($"Affected {objectsAffected} objects");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}