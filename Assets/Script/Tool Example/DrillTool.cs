using Script.Tool_Example;
using UnityEngine;
using UnityEngine.XR;

public class DrillTool : MonoBehaviour
{
    [Header("Drill Settings")]
    public float maxDrillDistance = 0.15f;    
    public float drillDamage = 10f;           
    public float drillRadius = 0.05f;         
    public float drillInterval = 0.1f;        

    [Header("References")]
    public SoilGenerator soilGenerator;       
    public Transform drillTip;                
    public ParticleSystem drillParticles;     

    [Header("Debris Settings")]
    public GameObject dirtPrefab;          // Prefab ของเศษดิน
    public int dirtSpawnCount = 5;         // จำนวนเศษดินต่อครั้งเจาะ

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;           
    public LayerMask fossilLayerMask;         

    private float nextDrillTime;              
    private bool isDrilling;                  
    private InputDevice rightHand;            

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

        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            if (!isDrilling)
                StartDrilling();

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

        if (Physics.Raycast(drillTip.position, drillTip.up, out RaycastHit hit, maxDrillDistance, soilLayerMask | fossilLayerMask))
        {
            // เจอ Soil
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Soil"))
            {
                SoilBlock soilBlock = hit.collider.GetComponent<SoilBlock>();
                if (soilBlock != null)
                    soilBlock.TakeDamage(drillDamage);

                soilGenerator.ClearBlocksInArea(hit.point, drillRadius);

                // 🔹 Spawn DirtChunk พร้อม Tag
                SpawnDirt(hit.point);
            }
            // เจอ Fossil
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Fossil"))
            {
                Fossil fossil = hit.collider.GetComponent<Fossil>();
                if (fossil != null)
                    fossil.TakeDamage(drillDamage, ToolType.ElectricDrill);
            }

            if (drillParticles != null)
            {
                drillParticles.transform.position = hit.point;
                drillParticles.transform.forward = hit.normal;
            }
        }
    }

    private void SpawnDirt(Vector3 position)
    {
        if (dirtPrefab == null) return;

        for (int i = 0; i < dirtSpawnCount; i++)
        {
            GameObject dirt = Instantiate(dirtPrefab, position, Random.rotation);
            dirt.tag = "DirtChunk"; // ตั้ง Tag ให้ Brush ลบได้

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
