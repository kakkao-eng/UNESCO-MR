using UnityEngine;
using UnityEngine.InputSystem;

public class DrillTool : MonoBehaviour
{
    [Header("Drill Settings")]
    public float maxDrillDistance = 2f;    // ระยะสูงสุดที่สว่านเจาะได้
    public float drillDamage = 0.1f;       // ความเสียหายต่อการเจาะ 1 ครั้ง
    public float drillInterval = 0.1f;      // ระยะเวลาระหว่างการเจาะแต่ละครั้ง
    
    [Header("References")]
    public SoilGenerator soilGenerator;
    public Transform drillTip;              // จุดปลายสว่าน
    public ParticleSystem drillParticles;   // เอฟเฟคการเจาะ (ถ้ามี)
    
    [Header("Visual Feedback")]
    public bool showDrillRange = true;
    public Color gizmoColor = Color.red;
    
    private float nextDrillTime;
    private bool isDrilling;
    
    private void Start()
    {
        if (soilGenerator == null)
            soilGenerator = FindObjectOfType<SoilGenerator>();
            
        if (drillTip == null)
            drillTip = transform;
            
        if (drillParticles != null)
            drillParticles.Stop();
    }

    private void Update()
    {
        // เริ่มเจาะเมื่อกด G
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            StartDrilling();
        }
        // หยุดเจาะเมื่อปล่อย G
        else if (Keyboard.current.gKey.wasReleasedThisFrame)
        {
            StopDrilling();
        }
        
        // ทำการเจาะต่อเนื่องถ้ากำลังเจาะอยู่
        if (isDrilling && Time.time >= nextDrillTime)
        {
            PerformDrill();
            nextDrillTime = Time.time + drillInterval;
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

        // ยิง Raycast จากปลายสว่าน
        RaycastHit hit;
        if (Physics.Raycast(drillTip.position, drillTip.forward, out hit, maxDrillDistance))
        {
            // เช็คว่าชนกับบล็อกดินหรือไม่
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer(soilGenerator.soilLayerName))
            {
                // เจาะเฉพาะบล็อกที่ชน
                soilGenerator.ClearBlocksInArea(hit.point, soilGenerator.blockSize * 0.5f);
                
                // ปรับตำแหน่งเอฟเฟคไปที่จุดที่เจาะ
                if (drillParticles != null)
                {
                    drillParticles.transform.position = hit.point;
                    drillParticles.transform.forward = hit.normal;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDrillRange || drillTip == null) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(drillTip.position, drillTip.position + drillTip.forward * maxDrillDistance);
    }
}