using UnityEngine;
using UnityEngine.InputSystem; // 👈 สำหรับ New Input System

public class DigOnG_InputSystem : MonoBehaviour
{
    [Header("Digging Settings")]
    public float range = 0.05f;
    public float damage = 0.5f;
    public LayerMask digMask; // เพิ่ม Layer Mask สำหรับเลือกว่าจะขุดชั้นดินอะไรได้บ้าง
    
    [Header("Feedback")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.green;

    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            PerformDig();
        }
    }

    void PerformDig()
    {
        // ใช้ OverlapSphere เพื่อตรวจจับบล็อกในรัศมี
        Collider[] hitBlocks = Physics.OverlapSphere(transform.position, range, digMask);
        int blocksAffected = 0;

        foreach (Collider col in hitBlocks)
        {
            SoilBlock block = col.GetComponent<SoilBlock>();
            if (block != null)
            {
                // เช็คประเภทของบล็อกก่อนทำลาย
                if (block.soilType == SoilType.Fossil)
                {
                    Debug.LogWarning("Found fossil! Be careful!");
                    // ให้ damage น้อยลงถ้าเป็นฟอสซิล
                    block.TakeDamage(damage * 0.5f);
                }
                else if (block.soilType == SoilType.NearFossil)
                {
                    Debug.Log("Near fossil! Use more precise tools!");
                    block.TakeDamage(damage * 0.75f);
                }
                else
                {
                    block.TakeDamage(damage);
                }
                
                blocksAffected++;
                Debug.Log($"Hit Block at {col.transform.position}, Type: {block.soilType}");
            }
        }

        if (blocksAffected > 0)
        {
            // อาจเพิ่มเสียงหรือ particle effect ตรงนี้
            Debug.Log($"Dug {blocksAffected} blocks");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}