using UnityEngine;
using UnityEngine.XR;

public class ChiselTool : MonoBehaviour
{
    [Header("Tool Settings")]
    public float maxDistance = 2f; // ระยะสูงสุดของ Raycast สำหรับสิ่ว

    [Header("References")]
    public Transform toolTip;      // จุดปลายสิ่ว (Tip) ของเครื่องมือ
    public ParticleSystem hitEffect; // เอฟเฟกต์ตอนโดนดินหรือฟอสซิล

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;   // Layer ของดิน
    public LayerMask fossilLayerMask; // Layer ของฟอสซิล

    [HideInInspector] public bool isAiming = false; // เช็คว่าสิ่วกำลังเล็งโดนวัตถุหรือไม่
    [HideInInspector] public Vector3 aimPoint;      // จุดที่ Raycast ชนวัตถุ

    public static ChiselTool activeChisel; // สิ่วที่ถูกใช้งาน/เล็งอยู่ ปัจจุบันในฉาก

    private void Update()
    {
        UpdateChisel(); // อัปเดตการยิง Raycast ทุกเฟรม
    }

    private void UpdateChisel()
    {
        // ยิง Raycast จากปลายสิ่วไปข้างหน้า
        if (Physics.Raycast(toolTip.position, toolTip.forward, out RaycastHit hit, maxDistance, soilLayerMask | fossilLayerMask))
        {
            isAiming = true;          // กำลังเล็งโดนวัตถุ
            aimPoint = hit.point;      // บันทึกตำแหน่งที่ชน
            activeChisel = this;       // กำหนดว่านี่คือสิ่วที่ถูกเล็งอยู่
        }
        else
        {
            isAiming = false;          // ไม่โดนวัตถุ
            if (activeChisel == this)
                activeChisel = null;   // รีเซ็ตสิ่วที่ active
        }
    }

#if UNITY_EDITOR
    // แสดง Gizmos ใน Scene View เท่านั้น (ไม่แสดงใน Game View)
    private void OnDrawGizmos()
    {
        if (toolTip == null) return; // ถ้าไม่ได้กำหนด toolTip จะไม่วาด

        // วาดเส้นสีฟ้าจาก toolTip → ระยะ maxDistance
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(toolTip.position, toolTip.position + toolTip.forward * maxDistance);

        // วาดจุดปลาย Raycast สีฟ้า (ปลายสิ่ว)
        Gizmos.DrawSphere(toolTip.position + toolTip.forward * maxDistance, 0.02f);

        // ถ้ากำลังเล็งโดนวัตถุ ให้แสดงจุดสีเหลืองรอบ aimPoint
        if (isAiming)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(aimPoint, 0.2f); // ขนาด 0.2 หน่วย
        }
    }
#endif
}
