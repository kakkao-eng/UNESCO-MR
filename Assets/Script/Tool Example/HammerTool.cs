using Script.Tool_Example;
using UnityEngine;
using UnityEngine.XR;

public class HammerTool_Adjustable : MonoBehaviour
{
    [Header("Hammer Settings")]
    public float maxHitDistance = 0.5f;   // ระยะตีสิ่ว
    public float hitRadius = 0.15f;        // รัศมีทำลายดิน
    public float hitForce = 20f;          // ดาเมจ

    [Header("Tip Offset")]
    public Vector3 toolTipOffset = new Vector3(0, 0, 0.1f); // ปรับตำแหน่งปลายค้อน

    [Header("Debug Visualization")]
    public bool showGizmo = true;         // เปิด/ปิด Gizmos

    [Header("References")]
    public Transform toolTip;              // จุดอ้างอิงปลายค้อน

    private InputDevice rightHand;
    private Vector3 lastHitPosition;

    private void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        UpdateHammer();
    }

    private void UpdateHammer()
    {
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed)
            && triggerPressed
            && ChiselTool.activeChisel != null)
        {
            float distanceToChisel = Vector3.Distance(transform.position, ChiselTool.activeChisel.toolTip.position);
            if (distanceToChisel <= maxHitDistance)
                HitWithHammer();
        }
    }

    private void HitWithHammer()
    {
        var chisel = ChiselTool.activeChisel;
        if (chisel == null || !chisel.isAiming) return;

        // คำนวณตำแหน่งปลายค้อน + Offset
        Vector3 hitPosition = toolTip.position + toolTip.TransformDirection(toolTipOffset);
        lastHitPosition = hitPosition;

        // เล่นเอฟเฟกต์ตี
        if (chisel.hitEffect != null)
        {
            chisel.hitEffect.transform.position = chisel.aimPoint;
            chisel.hitEffect.Play();
        }

        // เล่นเสียงค้อน
        SoundManager.PlaySound(SoundType.Hammer);

        // SphereCast ทำดาเมจ
        RaycastHit[] hits = Physics.SphereCastAll(
            hitPosition, 
            hitRadius, 
            chisel.toolTip.forward, 
            0.2f, 
            chisel.soilLayerMask | chisel.fossilLayerMask
        );

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<SoilBlock>(out var soilBlock))
                soilBlock.TakeDamage(hitForce);
            else if (hit.collider.TryGetComponent<Fossil>(out var fossil))
                fossil.TakeDamage(hitForce, ToolType.Chisel);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmo || toolTip == null) return;

        // จุดปลายค้อน + Offset
        Vector3 drawPosition = toolTip.position + toolTip.TransformDirection(toolTipOffset);

        // วาดรัศมีทำลาย
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawSphere(drawPosition, hitRadius);

        // วาดเส้นเชื่อมค้อน → สิ่วถ้ามี
        if (ChiselTool.activeChisel != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(drawPosition, ChiselTool.activeChisel.toolTip.position);
        }
    }
#endif
}
