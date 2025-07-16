using Script.Tool_Example;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrillTool : MonoBehaviour
{
    [Header("Drill Settings")]
    public float maxDrillDistance = 2f;    
    public float drillDamage = 20f;       // เพิ่มค่าความเสียหาย
    public float drillRadius = 0.2f;      // รัศมีการขุด
    public float drillInterval = 0.1f;    // ความถี่ในการขุด
    
    [Header("References")]
    public SoilGenerator soilGenerator;
    public Transform drillTip;              
    public ParticleSystem drillParticles;   
    
    [Header("Layer Settings")]
    public LayerMask soilLayerMask;     // Layer ของดิน
    public LayerMask fossilLayerMask;   // Layer ของฟอสซิล
    
    [Header("Debug Visualization")]
    public bool showDrillRange = true;
    public Color drillRangeColor = Color.red;
    
    private float nextDrillTime;
    private bool isDrilling;
    private bool isEquipped = false;

    private void Start()
    {
        if (soilGenerator == null)
            soilGenerator = FindObjectOfType<SoilGenerator>();
            
        if (drillTip == null)
            drillTip = transform;
            
        if (drillParticles != null)
            drillParticles.Stop();

        // กำหนด Layer Masks
        soilLayerMask = 1 << LayerMask.NameToLayer("Soil");
        fossilLayerMask = 1 << LayerMask.NameToLayer("Fossil");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            isEquipped = !isEquipped;
            Debug.Log($"Drill {(isEquipped ? "Equipped" : "Unequipped")}");
        }

        if (!isEquipped)
        {
            if (isDrilling) StopDrilling();
            return;
        }

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
    }

    private void StopDrilling()
    {
        isDrilling = false;
        if (drillParticles != null)
            drillParticles.Stop();
    }

    private void PerformDrill()
    {
        if (soilGenerator == null) return;

        RaycastHit hit;
        if (Physics.Raycast(drillTip.position, drillTip.forward, out hit, maxDrillDistance, soilLayerMask | fossilLayerMask))
        {
            // ตรวจสอบว่าชนกับอะไร
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Soil"))
            {
                // ถ้าเป็นดิน
                SoilBlock soilBlock = hit.collider.GetComponent<SoilBlock>();
                if (soilBlock != null)
                {
                    soilBlock.TakeDamage(drillDamage);
                }
                
                // ลบบล็อกดินในรัศมี
                soilGenerator.ClearBlocksInArea(hit.point, drillRadius);
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Fossil"))
            {
                // ถ้าเป็นฟอสซิล
                Fossil fossil = hit.collider.GetComponent<Fossil>();
                if (fossil != null)
                {
                    fossil.TakeDamage(drillDamage, ToolType.ElectricDrill);
                }
            }

            // แสดง particle effect ที่จุดชน
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
        Gizmos.DrawLine(drillTip.position, drillTip.position + drillTip.forward * maxDrillDistance);
        
        // วาดรัศมีการขุด
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Vector3 endPoint = drillTip.position + drillTip.forward * maxDrillDistance;
        Quaternion rotation = Quaternion.LookRotation(drillTip.forward);
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