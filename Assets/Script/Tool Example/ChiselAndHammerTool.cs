using Script.Tool_Example;
using UnityEngine;
using UnityEngine.XR;

public class ChiselAndHammerTool : MonoBehaviour
{
    public enum ToolMode { Chisel, Hammer } // โหมดเครื่องมือ

    [Header("Tool Settings")]
    public ToolMode currentMode;       // โหมดปัจจุบัน
    public float maxDistance = 2f;     // ระยะ Raycast สูงสุด
    public float hitRadius = 0.2f;     // รัศมีการตี
    public float hitForce = 5f;       // ดาเมจจากการตี
    public float maxHitDistance = 0.5f;// ระยะสูงสุดค้อนตีสิ่วได้

    [Header("References")]
    public Transform toolTip;          // จุดปลายสิ่ว/ค้อน
    public SoilGenerator soilGenerator;// ตัวจัดการดิน
    public ParticleSystem hitEffect;   // เอฟเฟกต์เวลาโดนตี

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;    
    public LayerMask fossilLayerMask;  

    // ตัวช่วยสำหรับสิ่ว
    private bool isAiming = false;             // เช็คว่ากำลังเล็งสิ่วหรือไม่
    private Vector3 aimPoint;                  // จุดที่ Raycast ชน
    private static ChiselAndHammerTool activeChisel; // สิ่วที่กำลังใช้งาน (ใช้คู่กับค้อน)

    private InputDevice rightHand; // ตัวแทนมือขวา Pico

    private void Start()
    {
        // หา SoilGenerator ถ้าไม่ได้อ้างอิง
        if (soilGenerator == null)
            soilGenerator = FindObjectOfType<SoilGenerator>();

        // ดึง Input ของมือขวา Pico
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        if (currentMode == ToolMode.Chisel)
            UpdateChisel(); // ทำงานโหมดสิ่ว
        else if (currentMode == ToolMode.Hammer)
            UpdateHammer(); // ทำงานโหมดค้อน
    }

    private void UpdateChisel()
    {
        // ยิง Raycast จากปลายสิ่วไปข้างหน้า
        if (Physics.Raycast(toolTip.position, toolTip.forward, out RaycastHit hit, maxDistance, soilLayerMask | fossilLayerMask))
        {
            isAiming = true;
            aimPoint = hit.point;
            activeChisel = this; // กำหนดว่านี่คือสิ่วที่ถูกเล็งอยู่
        }
        else
        {
            isAiming = false;
            if (activeChisel == this)
                activeChisel = null;
        }
    }

    private void UpdateHammer()
    {
        // ตรวจสอบ Trigger มือขวา กดตีค้อน
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed && activeChisel != null)
        {
            // เช็คว่าค้อนอยู่ใกล้สิ่วพอหรือไม่
            float distanceToChisel = Vector3.Distance(transform.position, activeChisel.toolTip.position);
            if (distanceToChisel <= maxHitDistance)
                HitWithHammer(); // ตีสิ่ว
        }
    }

    private void HitWithHammer()
    {
        if (!isAiming || activeChisel == null) return;

        // เล่นเอฟเฟกต์ตี
        if (hitEffect != null)
        {
            hitEffect.transform.position = aimPoint;
            hitEffect.Play();
        }

        // เล่นเสียงค้อน
        SoundManager.PlaySound(SoundType.Hammer);

        // ทำดาเมจต่อ Soil หรือ Fossil ด้วย SphereCast
        RaycastHit[] hits = Physics.SphereCastAll(activeChisel.toolTip.position, hitRadius, activeChisel.toolTip.forward, maxDistance, soilLayerMask | fossilLayerMask);

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<SoilBlock>(out var soilBlock))
                soilBlock.TakeDamage(hitForce); // ดาเมจดิน
            else if (hit.collider.TryGetComponent<Fossil>(out var fossil))
                fossil.TakeDamage(hitForce, ToolType.Chisel); // ดาเมจฟอสซิล
        }
    }

    // ✅ แสดงระยะเล็งเฉพาะใน Scene View (Editor)
    private void OnDrawGizmos()
    {
        if (toolTip == null) return;

        // เส้น Raycast สีฟ้า = ระยะเล็งสิ่ว
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(toolTip.position, toolTip.position + toolTip.forward * maxDistance);

        // จุดปลาย Ray
        Gizmos.DrawSphere(toolTip.position + toolTip.forward * maxDistance, 0.02f);

        // ถ้ามีการเล็งอยู่ แสดงรัศมีการตีสีเหลือง
        if (isAiming)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(aimPoint, hitRadius);
        }
    }
}
