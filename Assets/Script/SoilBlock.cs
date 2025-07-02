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
    public LayerMask fossilLayer;

    void Start()
    {
        rend = GetComponent<Renderer>();
        CheckNearbyFossils(); // เช็คฟอสซิลตั้งแต่เริ่มต้น
        UpdateColor();
    }

    public void CheckNearbyFossils()
    {
        // เช็คว่าบล็อกนี้อยู่ข้างใน Fossil ไหม
        Collider[] nearbyColliders = Physics.OverlapBox(
            transform.position,
            transform.localScale / 2, // ใช้ขนาดของบล็อกเป็นเกณฑ์
            transform.rotation,
            fossilLayer
        );

        if (nearbyColliders.Length > 0)
        {
            // ถ้าเจอว่าทับซ้อนกับ Fossil ให้ทำลายบล็อกนี้ทันที
            Destroy(gameObject);
            return;
        }

        // เช็คบล็อกที่อยู่ใกล้ Fossil (แต่ไม่ได้ทับซ้อน)
        nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius, fossilLayer);
        if (nearbyColliders.Length > 0)
        {
            soilType = SoilType.NearFossil;
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
        if (soilType == SoilType.NearFossil || soilType == SoilType.Fossil)
        {
            Destroy(gameObject);
        }
    }

    void UpdateColor()
    {
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
}
