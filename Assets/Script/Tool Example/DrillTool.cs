using Script.Tool_Example;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrillTool : MonoBehaviour
{
    [Header("Drill Settings")]
    public float maxDrillDistance = 2f;    // ระยะเจาะสูงสุด
    public float drillDamage = 20f;        // ดาเมจของสว่าน
    public float drillRadius = 0.2f;       // รัศมีผลกระทบ
    public float drillInterval = 0.1f;     // เวลาหน่วงระหว่างการเจาะ

    [Header("References")]
    public SoilGenerator soilGenerator;    // ตัวจัดการดิน
    public Transform drillTip;             // ปลายสว่าน
    public ParticleSystem drillParticles;  // เอฟเฟกต์เจาะ

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;        // เลเยอร์ดิน
    public LayerMask fossilLayerMask;      // เลเยอร์ฟอสซิล

    [Header("Debug Visualization")]
    public bool showDrillRange = true;     // แสดง Debug Line
    public Color drillRangeColor = Color.red;

    private float nextDrillTime;           // เวลาเจาะครั้งถัดไป
    private bool isDrilling;               // กำลังเจาะหรือไม่

    private void Start()
    {
        if (soilGenerator == null)
            soilGenerator = FindObjectOfType<SoilGenerator>();

        if (drillTip == null)
            drillTip = transform;

        if (drillParticles != null)
            drillParticles.Stop();

        soilLayerMask = 1 << LayerMask.NameToLayer("Soil");
        fossilLayerMask = 1 << LayerMask.NameToLayer("Fossil");
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (!isDrilling)
            {
                StartDrilling();
            }

            if (Time.time >= nextDrillTime)
            {
                PerformDrill();
                nextDrillTime = Time.time + drillInterval;
            }
        }
        else if (isDrilling)
        {
            StopDrilling();
        }
    }

    private void StartDrilling()
    {
        isDrilling = true;
        if (drillParticles != null)
            drillParticles.Play();
        SoundManager.PlayLoop(SoundType.Drill);
    }

    private void StopDrilling()
    {
        isDrilling = false;
        if (drillParticles != null)
            drillParticles.Stop();
        SoundManager.StopSound();
    }

    private void PerformDrill()
    {
        if (soilGenerator == null) return;

        RaycastHit hit;
        if (Physics.Raycast(drillTip.position, drillTip.up, out hit, maxDrillDistance, soilLayerMask | fossilLayerMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Soil"))
            {
                SoilBlock soilBlock = hit.collider.GetComponent<SoilBlock>();
                if (soilBlock != null) soilBlock.TakeDamage(drillDamage);
                soilGenerator.ClearBlocksInArea(hit.point, drillRadius);
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Fossil"))
            {
                Fossil fossil = hit.collider.GetComponent<Fossil>();
                if (fossil != null) fossil.TakeDamage(drillDamage, ToolType.ElectricDrill);
            }

            if (drillParticles != null)
            {
                drillParticles.transform.position = hit.point;
                drillParticles.transform.forward = hit.normal;
            }
        }
    }

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
