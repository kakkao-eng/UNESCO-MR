using Script.Tool_Example;
using UnityEngine;
using UnityEngine.XR;

public class DrillTool : MonoBehaviour
{
    [Header("Drill Settings")]
    public float maxDrillDistance = 0.15f;    // ระยะเจาะสูงสุด
    public float drillDamage = 20f;           // ดาเมจของสว่าน
    public float drillRadius = 0.05f;         // รัศมีผลกระทบ
    public float drillInterval = 0.1f;        // เวลาหน่วงระหว่างการเจาะ

    [Header("References")]
    public SoilGenerator soilGenerator;       // ตัวจัดการดิน
    public Transform drillTip;                // ปลายสว่าน
    public ParticleSystem drillParticles;     // เอฟเฟกต์เจาะ

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;           // เลเยอร์ดิน
    public LayerMask fossilLayerMask;         // เลเยอร์ฟอสซิล

    [Header("Debug Visualization")]
    public bool showDrillRange = true;        // แสดง Debug Line
    public Color drillRangeColor = Color.red;

    private float nextDrillTime;              // เวลาเจาะครั้งถัดไป
    private bool isDrilling;                  // กำลังเจาะหรือไม่
    private InputDevice rightHand;            // ตัวแทนมือขวา VR

    private void Start()
    {
        // หา SoilGenerator ถ้ายังไม่ได้อ้างอิง
        if (soilGenerator == null)
            soilGenerator = FindObjectOfType<SoilGenerator>();

        // ถ้าไม่มี drillTip ให้ใช้ Transform ของสว่านเอง
        if (drillTip == null)
            drillTip = transform;

        // ปิด Particle ตอนเริ่ม
        if (drillParticles != null)
            drillParticles.Stop();

        // กำหนดเลเยอร์สำหรับ Raycast
        soilLayerMask = 1 << LayerMask.NameToLayer("Soil");
        fossilLayerMask = 1 << LayerMask.NameToLayer("Fossil");

        // ดึง Input ของมือขวา Pico
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        // ตรวจสอบว่ากด Trigger มือขวาอยู่หรือไม่
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            if (!isDrilling)
                StartDrilling(); // เริ่มเจาะ

            // เวลาผ่าน interval แล้วให้ PerformDrill
            if (Time.time >= nextDrillTime)
            {
                PerformDrill();
                nextDrillTime = Time.time + drillInterval;
            }
        }
        else if (isDrilling)
        {
            StopDrilling(); // หยุดเจาะถ้าไม่ได้กด
        }
    }

    private void StartDrilling()
    {
        isDrilling = true;
        if (drillParticles != null)
            drillParticles.Play(); // เล่น Particle
        SoundManager.PlayLoop(SoundType.Drill); // เล่นเสียงสว่านวนลูป
    }

    private void StopDrilling()
    {
        isDrilling = false;
        if (drillParticles != null)
            drillParticles.Stop(); // หยุด Particle
        SoundManager.StopSound(); // หยุดเสียง
    }

    private void PerformDrill()
    {
        if (soilGenerator == null) return;

        // ยิง Raycast จากปลายสว่านไปข้างหน้า
        if (Physics.Raycast(drillTip.position, drillTip.up, out RaycastHit hit, maxDrillDistance, soilLayerMask | fossilLayerMask))
        {
            // ตรวจสอบชน Soil
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Soil"))
            {
                SoilBlock soilBlock = hit.collider.GetComponent<SoilBlock>();
                if (soilBlock != null)
                    soilBlock.TakeDamage(drillDamage); // ทำดาเมจ
                soilGenerator.ClearBlocksInArea(hit.point, drillRadius); // ลบดินรอบ ๆ
            }
            // ตรวจสอบชน Fossil
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Fossil"))
            {
                Fossil fossil = hit.collider.GetComponent<Fossil>();
                if (fossil != null)
                    fossil.TakeDamage(drillDamage, ToolType.ElectricDrill); // ทำดาเมจฟอสซิล
            }

            // อัปเดตตำแหน่ง Particle
            if (drillParticles != null)
            {
                drillParticles.transform.position = hit.point;
                drillParticles.transform.forward = hit.normal;
            }
        }
    }

    // วาด Gizmos สำหรับ Debug
    private void OnDrawGizmos()
    {
        if (!showDrillRange || drillTip == null) return;

        Gizmos.color = drillRangeColor;
        Gizmos.DrawLine(drillTip.position, drillTip.position + drillTip.up * maxDrillDistance);

        Matrix4x4 originalMatrix = Gizmos.matrix;
        Vector3 endPoint = drillTip.position + drillTip.up * maxDrillDistance;
        Quaternion rotation = Quaternion.LookRotation(drillTip.up);
        Gizmos.matrix = Matrix4x4.TRS(endPoint, rotation, Vector3.one);
        DrawGizmosCircle(Vector3.zero, drillRadius, 32);
        Gizmos.matrix = originalMatrix;
    }

    // วาดวงกลมรัศมี drill
    private void DrawGizmosCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + Vector3.right * radius;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }
}
