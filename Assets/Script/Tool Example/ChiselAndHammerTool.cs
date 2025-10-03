using Script.Tool_Example;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChiselAndHammerTool : MonoBehaviour
{
    public enum ToolMode
    {
        Chisel, // โหมดสิ่ว
        Hammer  // โหมดค้อน
    }

    [Header("Tool Settings")]
    public ToolMode currentMode;       // โหมดปัจจุบัน (สลับสิ่ว/ค้อน)
    public float maxDistance = 2f;     // ระยะสูงสุดของ Raycast
    public float hitRadius = 0.2f;     // รัศมีการตี
    public float hitForce = 20f;       // ดาเมจจากการตี
    public float maxHitDistance = 0.5f;// ระยะสูงสุดที่ค้อนจะตีสิ่วได้

    [Header("References")]
    public Transform toolTip;          // จุดปลายของสิ่ว/ค้อน
    public SoilGenerator soilGenerator;// อ้างถึงตัวที่จัดการดิน
    public ParticleSystem hitEffect;   // เอฟเฟกต์เวลาโดนตี

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;
    public LayerMask fossilLayerMask;

    // ตัวช่วยสำหรับสิ่ว
    private bool isAiming = false;     // เช็คว่ากำลังเล็งสิ่วหรือไม่
    private Vector3 aimPoint;          // จุดที่ Raycast ชน
    private static ChiselAndHammerTool activeChisel; // สิ่วที่กำลังใช้งาน (ใช้คู่กับค้อน)

    private void Start()
    {
        if (soilGenerator == null)
            soilGenerator = FindObjectOfType<SoilGenerator>();
    }

    private void Update()
    {
        if (currentMode == ToolMode.Chisel)
        {
            UpdateChisel(); // ทำงานโหมดสิ่ว
        }
        else if (currentMode == ToolMode.Hammer)
        {
            UpdateHammer(); // ทำงานโหมดค้อน
        }
    }

    private void UpdateChisel()
    {
        // ยิง Raycast จากปลายสิ่วไปข้างหน้า
        RaycastHit hit;
        if (Physics.Raycast(toolTip.position, toolTip.forward, out hit, maxDistance, soilLayerMask | fossilLayerMask))
        {
            isAiming = true;
            aimPoint = hit.point;
            activeChisel = this; // กำหนดว่านี่คือสิ่วที่ถูกเล็งอยู่

            // เส้น debug
            Debug.DrawLine(toolTip.position, hit.point, Color.yellow);
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
        // ถ้ากดคลิกซ้าย และมีสิ่วที่เล็งอยู่
        if (Mouse.current.leftButton.wasPressedThisFrame && activeChisel != null)
        {
            // เช็คว่าค้อนอยู่ใกล้สิ่วพอหรือไม่
            float distanceToChisel = Vector3.Distance(transform.position, activeChisel.toolTip.position);
            if (distanceToChisel <= maxHitDistance)
            {
                HitWithHammer(); // ตีสิ่ว
            }
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

        SoundManager.PlaySound(SoundType.Hammer);

        // ทำดาเมจต่อ Soil หรือ Fossil
        RaycastHit[] hits = Physics.SphereCastAll(
            activeChisel.toolTip.position, 
            hitRadius, 
            activeChisel.toolTip.forward, 
            maxDistance, 
            soilLayerMask | fossilLayerMask
        );

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<SoilBlock>(out var soilBlock))
            {
                soilBlock.TakeDamage(hitForce);
            }
            else if (hit.collider.TryGetComponent<Fossil>(out var fossil))
            {
                fossil.TakeDamage(hitForce, ToolType.Chisel);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (isAiming)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(aimPoint, hitRadius);
        }
    }
}
