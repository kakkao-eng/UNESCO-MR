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

    private AudioSource drillAudio; // 🔊 เพิ่ม AudioSource สำหรับเสียงเจาะเฉพาะตัว

    private void Start()
    {
        if (soilGenerator == null)
            soilGenerator = FindObjectOfType<SoilGenerator>();

        if (drillTip == null)
            drillTip = transform;

        if (drillParticles != null)
            drillParticles.Stop();

        // ตั้ง Layer
        soilLayerMask = 1 << LayerMask.NameToLayer("Soil");
        fossilLayerMask = 1 << LayerMask.NameToLayer("Fossil");

        // ดึง Input มือขวา
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // 🎧 เพิ่ม AudioSource เฉพาะตัวเครื่องมือ
        drillAudio = gameObject.AddComponent<AudioSource>();
        drillAudio.loop = true;
        drillAudio.playOnAwake = false;
        drillAudio.volume = 1f;

        // ✅ โหลดเสียงจาก SoundManager (เสียงแรกของ Drill)
        var clips = FindObjectOfType<SoundManager>()?.GetClip(SoundType.Drill);
        if (clips != null)
            drillAudio.clip = clips;
    }

    private void Update()
    {
        // ต้องถืออยู่ถึงจะทำงาน
        if (!isGrabbed)
        {
            if (isDrilling) StopDrilling();
            return;
        }

        // ตรวจจับ Trigger
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
        else
        {
            if (isDrilling)
                StopDrilling();
        }
    }

    private void StartDrilling()
    {
        isDrilling = true;

        if (drillParticles != null)
            drillParticles.Play();

        // 🔊 เล่นเสียงเจาะต่อเนื่อง (เฉพาะ Drill)
        if (drillAudio != null && !drillAudio.isPlaying)
            drillAudio.Play();
    }

    private void StopDrilling()
    {
        isDrilling = false;

        if (drillParticles != null)
            drillParticles.Stop();

        // 🔇 หยุดเสียงทันทีเมื่อปล่อย Trigger
        if (drillAudio != null && drillAudio.isPlaying)
            drillAudio.Stop();
    }

    private void PerformDrill()
    {
        if (soilGenerator == null) return;

        if (Physics.Raycast(drillTip.position, drillTip.up, out RaycastHit hit, maxDrillDistance, soilLayerMask | fossilLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            // ❌ ไม่ให้ขุดซ้ำเศษดิน
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

    public void OnGrabbed()
    {
        isGrabbed = true;
        Debug.Log("🪓 Drill grabbed!");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        StopDrilling();
        Debug.Log("🪓 Drill released!");
    }
}
