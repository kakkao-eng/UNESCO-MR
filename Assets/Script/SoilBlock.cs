using UnityEngine;
using System.Collections;

public enum SoilType { Normal, NearFossil, Fossil, Damaged }

public class SoilBlock : MonoBehaviour
{
    [Header("Soil Properties")]
    public float density = 1.0f;  // ความหนาแน่นของดิน
    public float moisture = 0.5f; // ความชื้น
    public Color baseColor = Color.grey; // สีพื้นฐาน

    [Header("Soil Behavior")]
    public bool usePhysics = true;
    public float crumbleThreshold = 0.3f; // เริ่มร่วงเมื่อ health ต่ำกว่านี้
    public GameObject dirtParticlePrefab; // particle effect สำหรับดินร่วง

    public SoilType soilType = SoilType.Normal;
    public float health = 1.0f;  // 1.0 = สมบูรณ์, 0 = พัง
    private Renderer rend;

    // เพิ่ม field ใหม่
    public float detectionRadius = 0.15f;

    public event System.Action OnDestroyEvent;

    [Header("Layer Settings")]
    [Tooltip("Layer ที่จะใช้ตรวจจับ Fossil")]
    public LayerMask fossilLayer = 1 << 6; // default to "Fossil" layer
    
    [Tooltip("Layer ที่จะกำหนดให้กับ soil block")]
    public string soilLayerName = "Soil"; // default layer name for soil blocks

    private Fossil nearbyFossil; // เพิ่มตัวแปรเก็บ reference ไปยัง Fossil ที่อยู่ใกล้

    // Add this method right after the field declarations
    private void InitializeRenderer()
    {
        if (rend != null)
            return;
        
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError($"Missing Renderer component on {gameObject.name}. Adding MeshRenderer.", this);
            rend = gameObject.AddComponent<MeshRenderer>();
        }

        // Ensure material is set
        if (rend.material == null)
        {
            rend.material = new Material(Shader.Find("Standard"));
        }
    }

    // Modify Start method
    void Start()
    {
        // ตรวจสอบความถูกต้องของ Layer
        if (LayerMask.NameToLayer("Fossil") == -1)
        {
            Debug.LogError("Missing 'Fossil' layer! Please add this layer in Project Settings.");
        }
        
        if (LayerMask.NameToLayer(soilLayerName) == -1)
        {
            Debug.LogError($"Missing '{soilLayerName}' layer! Please add this layer in Project Settings.");
        }

        // กำหนด layer ให้กับ soil block
        gameObject.layer = LayerMask.NameToLayer(soilLayerName);
        
        InitializeRenderer();
        CheckNearbyFossils();
        UpdateColor();
    }

    // Modify CheckNearbyFossils method to ensure renderer is initialized
    public void CheckNearbyFossils()
    {
        InitializeRenderer(); // Add this line

        // เพิ่ม Debug Log เพื่อตรวจสอบ
        if (fossilLayer.value == 0)
        {
            Debug.LogWarning("FossilLayer is not set properly!");
            return;
        }

        // เช็คว่าบล็อกนี้อยู่ข้างใน Fossil ไหม
        Collider[] nearbyColliders = Physics.OverlapBox(
            transform.position,
            transform.localScale / 2, // ใช้ขนาดของบล็อกเป็นเกณฑ์
            transform.rotation,
            fossilLayer
        );

        if (nearbyColliders.Length > 0)
        {
            foreach (var col in nearbyColliders)
            {
                // เพิ่ม Debug Log เพื่อตรวจสอบ
                Debug.Log($"Found fossil collision with: {col.gameObject.name} on layer: {LayerMask.LayerToName(col.gameObject.layer)}");
            }
            Destroy(gameObject);
            return;
        }

        // เช็คบล็อกที่อยู่ใกล้ Fossil (แต่ไม่ได้ทับซ้อน)
        nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius, fossilLayer);
        if (nearbyColliders.Length > 0)
        {
            soilType = SoilType.NearFossil;
            nearbyFossil = nearbyColliders[0].GetComponent<Fossil>(); // เพิ่มการเก็บ reference ไปยัง Fossil
            UpdateColor(); // เพิ่มการอัพเดทสีทันทีเมื่อเจอ Fossil
        }
    }

    public void TakeDamage(float amount)
    {
        // Kicks off the damage logic without overriding any base method
        float actualDamage = amount / density;
        health -= actualDamage;

        if (dirtParticlePrefab != null)
        {
            GameObject hitParticles = Instantiate(dirtParticlePrefab, transform.position, Quaternion.identity);
            var mainModule = hitParticles.GetComponent<ParticleSystem>().main;
            mainModule.startColor = rend.material.color;
            Destroy(hitParticles, 1f);
        }

        UpdateSoilAppearance();

        if (health <= 0)
        {
            if (soilType == SoilType.Fossil)
            {
                soilType = SoilType.Damaged;
                health = 0.5f;
                Debug.LogWarning("Fossil damaged! Use repair tool!");
            }
            else
            {
                StartCrumbling();
            }
        }
    }

    public void Repair()
    {
        if (soilType == SoilType.Damaged)
        {
            health = 1.0f;
            soilType = SoilType.Fossil;
            UpdateColor();
        }
    }

    public void Brush()
    {
        if (soilType == SoilType.NearFossil)
        {
            // สร้าง particle effect ฝุ่น
            if (dirtParticlePrefab != null)
            {
                GameObject particles = Instantiate(dirtParticlePrefab, transform.position, Quaternion.identity);
                var mainModule = particles.GetComponent<ParticleSystem>().main;
                mainModule.startColor = rend.material.color;
                Destroy(particles, 2f);
            }

            // ทำลายบล็อกดิน
            Destroy(gameObject);
        }
    }

    // Modify UpdateColor method to use InitializeRenderer
    void UpdateColor()
    {
        InitializeRenderer(); // Add this line
        
        // ตรวจสอบว่า rend ไม่เป็น null ก่อนใช้งาน
        if (rend == null)
        {
            Debug.LogError($"Failed to initialize Renderer on {gameObject.name}", this);
            return;
        }

        switch (soilType)
        {
            case SoilType.Normal:
                rend.material.color = Color.grey;
                break;
            case SoilType.NearFossil:
                rend.material.color = Color.yellow;
                break;
            case SoilType.Fossil:
                rend.material.color = Color.white;
                break;
            case SoilType.Damaged:
                rend.material.color = Color.red;
                break;
        }
    }

    private void UpdateSoilAppearance()
    {
        // ตรวจสอบว่า rend ไม่เป็น null ก่อนใช้งาน
        if (rend == null || rend.material == null)
        {
            Debug.LogError($"Renderer or material is null on {gameObject.name}", this);
            return;
        }

        Color finalColor = baseColor;

        // ปรับสีตามความชื้น
        float darkenAmount = Mathf.Lerp(0.2f, 0f, moisture);
        finalColor = Color.Lerp(finalColor, Color.black, darkenAmount);

        // ปรับความโปร่งใสตามสุขภาพ
        finalColor.a = Mathf.Lerp(0.5f, 1f, health);

        rend.material.color = finalColor;

        // ถ้าสุขภาพต่ำ เริ่มร่วง
        if (health < crumbleThreshold && usePhysics)
        {
            StartCrumbling();
        }
    }

    private void StartCrumbling()
    {
        if (dirtParticlePrefab != null)
        {
            // สร้าง particle effect ดินร่วง
            GameObject particles = Instantiate(dirtParticlePrefab, transform.position, Quaternion.identity);
            Destroy(particles, 2f); // ทำลายหลังจาก 2 วินาที
        }

        // เพิ่ม Rigidbody ชั่วคราวเพื่อให้ร่วงลงตามแรงโน้มถ่วง
        if (usePhysics && GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = density;
            rb.linearDamping = 1f;

            // ทำลายบล็อกหลังจากร่วงลงพื้น
            StartCoroutine(DestroyAfterFall());
        }
    }

    private IEnumerator DestroyAfterFall()
    {
        yield return new WaitForSeconds(0.5f);
        float startY = transform.position.y;

        while (true)
        {
            // รอจนกว่าจะหยุดเคลื่อนที่
            if (Mathf.Abs(transform.position.y - startY) < 0.01f)
            {
                Destroy(gameObject);
                break;
            }
            startY = transform.position.y;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnDestroy()
    {
        if (soilType == SoilType.NearFossil && nearbyFossil != null)
        {
            nearbyFossil.CheckSurroundingBlocks();
        }
        OnDestroyEvent?.Invoke();

        // เพิ่มบรรทัดนี้
        CheckYellowSoilAndEndGame();
    }

    // เพิ่มฟังก์ชันนี้ในคลาส SoilBlock (หรือจะสร้าง Utility class ก็ได้)
    private void CheckYellowSoilAndEndGame()
    {
        var yellowSoils = GameObject.FindObjectsOfType<SoilBlock>();
        foreach (var soil in yellowSoils)
        {
            if (soil.soilType == SoilType.NearFossil)
                return;
        }
        GameManager.Instance.CompleteGame();
    }
}