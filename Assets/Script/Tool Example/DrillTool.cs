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
    public GameObject dirtPrefab;
    public int dirtSpawnCount = 5;

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;
    public LayerMask fossilLayerMask;

    private float nextDrillTime;
    private bool isDrilling;
    private bool isGrabbed = false;
    private InputDevice rightHand;

    private bool soundPlaying = false;

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
        // ต้องถืออยู่ถึงจะทำงาน
        if (!isGrabbed)
        {
            if (isDrilling) StopDrilling();
            return;
        }

        // ตรวจจับการกด Trigger
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            if (!isDrilling)
            {
                StartDrilling();
                PlayDrillSound();
            }

            if (Time.time >= nextDrillTime)
            {
                PerformDrill();
                nextDrillTime = Time.time + drillInterval;
            }
        }
        else
        {
            if (isDrilling)
                StopDrilling();

            StopDrillSound();
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

    private void PlayDrillSound()
    {
        // ตรวจสอบก่อนว่าไม่ได้เล่นอยู่
        if (!soundPlaying)
        {
            SoundManager.PlayLoop(SoundType.Drill);
            soundPlaying = true;
        }
    }

    private void StopDrillSound()
    {
        // ตรวจสอบว่าเสียงสว่านกำลังเล่นหรือไม่ ก่อนสั่งหยุด
        if (soundPlaying) // ใช้ soundPlaying ที่เราควบคุมเอง
        {
            SoundManager.StopLoopSound(); // สั่งหยุด AudioSource ที่ใช้ Loop
            soundPlaying = false;
        }
    }

    // ✅ ฟังก์ชันหลัก — ขุดเฉพาะดินจริง / ฟอสซิล (ไม่ขุด DirtChunk)
    private void PerformDrill()
    {
        if (soilGenerator == null) return;

        if (Physics.Raycast(drillTip.position, drillTip.up, out RaycastHit hit, maxDrillDistance, soilLayerMask | fossilLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            // ❌ ถ้าเจอวัตถุที่เป็นเศษดิน (DirtChunk) → ไม่ต้องทำอะไร
            if (hitObject.CompareTag("DirtChunk"))
                return;

            // ✅ เจอ Soil block
            if (hitObject.layer == LayerMask.NameToLayer("Soil"))
            {
                if (hitObject.TryGetComponent(out SoilBlock soilBlock))
                    soilBlock.TakeDamage(drillDamage);

                soilGenerator.ClearBlocksInArea(hit.point, drillRadius);
                SpawnDirt(hit.point);
            }
            // ✅ เจอ Fossil
            else if (hitObject.layer == LayerMask.NameToLayer("Fossil"))
            {
                if (hitObject.TryGetComponent(out Fossil fossil))
                    fossil.TakeDamage(drillDamage, ToolType.ElectricDrill);
            }

            // ปรับตำแหน่งฝุ่น
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

    // ✅ Event จาก XR Grab Interactable
    public void OnGrabbed()
    {
        isGrabbed = true;
        Debug.Log("Drill grabbed!");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        StopDrilling();
        StopDrillSound();
        Debug.Log("Drill released!");
    }
}
