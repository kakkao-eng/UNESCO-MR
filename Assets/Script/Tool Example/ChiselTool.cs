using UnityEngine;

public class ChiselTool : MonoBehaviour
{
    [Header("Tool Settings")]
    public float maxDistance = 2f; // ระยะตรวจจับวัตถุ

    [Header("References")]
    public Transform toolTip;        // ปลายสิ่ว
    public ParticleSystem hitEffect; // เอฟเฟกต์เมื่อโดนดิน/ฟอสซิล

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;   // เลเยอร์ดิน
    public LayerMask fossilLayerMask; // เลเยอร์ฟอสซิล

    [HideInInspector] public bool isAiming = false; 
    [HideInInspector] public Vector3 aimPoint;      

    public static ChiselTool activeChisel; // สิ่วที่ใช้อยู่ปัจจุบัน

    private void Update()
    {
        // ยิง Ray ออกไปจากปลายสิ่วเพื่อตรวจหาวัตถุ
        if (Physics.Raycast(toolTip.position, toolTip.forward, out RaycastHit hit, maxDistance, soilLayerMask | fossilLayerMask))
        {
            isAiming = true;
            aimPoint = hit.point;
            activeChisel = this;
        }
        else
        {
            isAiming = false;
            if (activeChisel == this)
                activeChisel = null;
        }
    }
}
