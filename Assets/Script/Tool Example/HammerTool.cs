using Script.Tool_Example;
using UnityEngine;
using UnityEngine.XR;

public class HammerTool : MonoBehaviour
{
    [Header("Hammer Settings")]
    public float maxHitDistance = 0.5f;   // ระยะที่ค้อนสามารถตีถึงสิ่ว
    public float hitRadius = 0.15f;       // รัศมีการกระทบ
    public float hitForce = 5f;           // แรงตี

    [Header("Tip Offset")]
    public Vector3 toolTipOffset = new Vector3(0, 0, 0.1f); // จุดปลายค้อน

    [Header("References")]
    public Transform toolTip;              // จุดอ้างอิงปลายค้อน

    [Header("Debris Settings")]
    public GameObject dirtPrefab;          // พรีแฟบเศษดิน
    [Range(1, 3)]
    public int dirtSpawnCount = 2;         // ✅ Spawn ดินให้น้อยลง (จาก 5 → 2)

    private InputDevice rightHand;         // อ้างอิง Controller ด้านขวา

    private void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        // ตรวจว่ากด Trigger และมีสิ่วที่กำลัง Aim อยู่หรือไม่
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

        // คำนวณตำแหน่งกระแทก
        Vector3 hitPosition = toolTip.position + toolTip.TransformDirection(toolTipOffset);

        // เอฟเฟกต์ตอนตี
        if (chisel.hitEffect != null)
        {
            chisel.hitEffect.transform.position = chisel.aimPoint;
            chisel.hitEffect.Play();
        }

        // เล่นเสียงค้อน
        SoundManager.PlaySound(SoundType.Hammer);

        // ตรวจว่าสิ่งที่ตีเป็นดินหรือฟอสซิล
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

        // ✅ Spawn เศษดิน (จำนวนน้อย)
        SpawnDirt(hitPosition);
    }

    /// <summary>
    /// สร้างเศษดินหลังจากตี
    /// </summary>
    private void SpawnDirt(Vector3 position)
    {
        if (dirtPrefab == null) return;

        for (int i = 0; i < dirtSpawnCount; i++)
        {
            GameObject dirt = Instantiate(dirtPrefab, position, Random.rotation);
            dirt.tag = "DirtChunk"; // ให้ Brush ลบได้

            // เพิ่มแรงสุ่มให้ดินกระเด้งออกไปเล็กน้อย
            if (dirt.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 randomDir = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1.5f),
                    Random.Range(-1f, 1f)
                ).normalized;
                rb.AddForce(randomDir * Random.Range(1f, 3f), ForceMode.Impulse);
            }
        }
    }
}
