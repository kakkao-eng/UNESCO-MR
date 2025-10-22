using Script.Tool_Example;
using UnityEngine;
using UnityEngine.XR;

public class HammerTool : MonoBehaviour
{
    [Header("Hammer Settings")]
    public float maxHitDistance = 0.5f;
    public float hitRadius = 0.15f;
    public float hitForce = 5f;

    [Header("Tip Offset")]
    public Vector3 toolTipOffset = new Vector3(0, 0, 0.1f);

    [Header("References")]
    public Transform toolTip;

    [Header("Debris Settings")]
    public GameObject dirtPrefab;
    [Range(1, 3)]
    public int dirtSpawnCount = 2;

    private InputDevice rightHand;
    private bool isGrabbed = false;   // ✅ ต้องถือก่อนถึงใช้ได้

    // Cooldown
    private float nextHitTime = 0f;
    private float hitCooldown = 0.25f;

    private void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        // ❌ ถ้าไม่ได้ถือค้อน → ไม่ให้ทำงาน
        if (!isGrabbed)
            return;

        // ✅ ตรวจว่ากด Trigger และมีสิ่วที่กำลัง Aim อยู่
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed)
            && triggerPressed
            && ChiselTool.activeChisel != null)
        {
            float distanceToChisel = Vector3.Distance(transform.position, ChiselTool.activeChisel.toolTip.position);
            if (distanceToChisel <= maxHitDistance && Time.time >= nextHitTime)
            {
                HitWithHammer();
                nextHitTime = Time.time + hitCooldown;
            }
        }
    }

    private void HitWithHammer()
    {
        var chisel = ChiselTool.activeChisel;
        if (chisel == null || !chisel.isAiming) return;

        Vector3 hitPosition = toolTip.position + toolTip.TransformDirection(toolTipOffset);

        // เอฟเฟกต์ตอนตี
        if (chisel.hitEffect != null)
        {
            chisel.hitEffect.transform.position = chisel.aimPoint;
            chisel.hitEffect.Play();
        }

        // ✅ เสียงค้อน (เล่นได้แค่ตอนตี)
        SoundManager.PlaySound(SoundType.Hammer);

        // ตรวจการชน
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

        // Spawn ดิน
        SpawnDirt(hitPosition);
    }

    private void SpawnDirt(Vector3 position)
    {
        if (dirtPrefab == null) return;

        for (int i = 0; i < dirtSpawnCount; i++)
        {
            GameObject dirt = Instantiate(dirtPrefab, position, Random.rotation);
            dirt.tag = "DirtChunk";

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

    // ✅ เรียกจาก XR Grab Interactable
    public void OnGrabbed()
    {
        isGrabbed = true;
        Debug.Log("Hammer grabbed!");
    }

    // ✅ เรียกจาก XR Grab Interactable
    public void OnReleased()
    {
        isGrabbed = false;
        Debug.Log("Hammer released!");
    }
}
