using UnityEngine;
using System.Collections;
using Script.Tool_Example;

// ใช้ ToolType จาก ToolData.cs

public class Fossil : MonoBehaviour
{
    [Header("Fossil Properties")]
    [SerializeField] 
    [Tooltip("ID ของฟอสซิลใน FossilData")]
    private int fossilId;

    [SerializeField]
    [Tooltip("ID Model ของฟอสซิลใน FossilData")]
    private int ModelFossilID;

    [SerializeField]
    [Tooltip("ความทนทานของฟอสซิล")]
    private float durability = 100f;

    [SerializeField]
    [Tooltip("สถานะของฟอสซิล")]
    private FossilState currentState = FossilState.Buried;

    [Header("Visual Feedback")]
    [SerializeField]
    private Material damagedMaterial;
    [SerializeField]
    private Material cleanedMaterial;
    [SerializeField]
    private Material repairedMaterial;
    
    [Header("Effects")]
    [SerializeField]
    private ParticleSystem damageEffect;
    [SerializeField]
    private AudioClip damageSound;

    private Renderer fossilRenderer;
    private Material originalMaterial;
    private float currentDurability;

    [SerializeField]
    private float fadeOutDuration = 1f; // ระยะเวลาในการหายไป

    private bool isFading = false;

    [Header("Fall Settings")]
    [SerializeField] 
    private float requiredClearedBlocks = 4; // จำนวนบล็อกขั้นต่ำที่ต้องถูกขุด
    [SerializeField]
    private float fallSpeed = 5f; // ความเร็วในการตก
    private int clearedBlocksCount = 0;
    private bool isFalling = false;

    // สถานะของฟอสซิล
    public enum FossilState
    {
        Buried,      // ยังฝังอยู่ในดิน
        Excavated,   // ขุดพบแล้ว
        Cleaned,     // ทำความสะอาดแล้ว
        Damaged,     // เสียหาย
        Repaired,    // ซ่อมแซมแล้ว
        Collected    // เก็บเข้าคอลเลคชัน
    }

    void Start()
    {
        fossilRenderer = GetComponent<Renderer>();
        if (fossilRenderer != null)
            originalMaterial = fossilRenderer.material;
        
        currentDurability = durability;
    }

    public void TakeDamage(float damage, ToolType toolType)
    {
        if (currentState == FossilState.Collected || isFading)
            return;

        float actualDamage = CalculateDamage(damage, toolType);
        currentDurability -= actualDamage;

        // เล่นเอฟเฟกต์เมื่อได้รับความเสียหาย
        if (damageEffect != null)
            damageEffect.Play();

        if (currentDurability <= 0)
        {
            SetState(FossilState.Damaged);
            StartCoroutine(FadeOutAndDestroy());
        }

        UpdateVisuals();
    }

    private float CalculateDamage(float damage, ToolType toolType)
    {
        // คำนวณความเสียหายตามประเภทของเครื่องมือ
        switch (toolType)
        {
            case ToolType.ElectricDrill:
                return damage * 2f; // สว่านไฟฟ้าทำความเสียหายมาก
            case ToolType.Hammer:
            case ToolType.Chisel:
                return damage * 1.5f; // ค้อนหรือสิ่วทำความเสียหายปานกลาง
            case ToolType.Brush:
                return 0f; // แปรงไม่ทำความเสียหาย
            case ToolType.Glue:
                return 0f; // กาวไม่ทำความเสียหาย
            default:
                return damage;
        }
    }

    public void Clean()
    {
        if (currentState == FossilState.Excavated)
        {
            SetState(FossilState.Cleaned);
            UpdateVisuals();
        }
    }

    public void Repair()
    {
        if (currentState == FossilState.Damaged)
        {
            currentDurability = durability;
            SetState(FossilState.Repaired);
            UpdateVisuals();
        }
    }

    private void SetState(FossilState newState)
    {
        currentState = newState;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (fossilRenderer == null || isFading) return;

        // อัพเดทวัสดุตามสถานะ
        switch (currentState)
        {
            case FossilState.Buried:
                fossilRenderer.material = originalMaterial;
                break;
            case FossilState.Excavated:
                fossilRenderer.material = cleanedMaterial; // หรือใช้ material อื่นตามต้องการ
                break;
            case FossilState.Damaged:
                fossilRenderer.material = damagedMaterial;
                break;
            case FossilState.Cleaned:
                fossilRenderer.material = cleanedMaterial;
                break;
            case FossilState.Repaired:
                fossilRenderer.material = repairedMaterial;
                break;
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isFading = true;

        // เปลี่ยนเป็นวัสดุสีแดง
        if (fossilRenderer != null && damagedMaterial != null)
        {
            fossilRenderer.material = damagedMaterial;
        }

        // รอสักครู่ให้เห็นสีแดง
        yield return new WaitForSeconds(0.5f);

        // ทำการ fade out
        float elapsed = 0;
        Color startColor = fossilRenderer.material.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeOutDuration;

            // ปรับความโปร่งใส
            Color currentColor = Color.Lerp(startColor, endColor, normalizedTime);
            fossilRenderer.material.color = currentColor;

            yield return null;
        }

        // ทำลาย GameObject
        Destroy(gameObject);
    }

    public void CheckSurroundingBlocks()
    {
        // ตรวจสอบบล็อกสีเหลืองที่เหลืออยู่รอบๆ
        Collider[] nearbyBlocks = Physics.OverlapSphere(
            transform.position, 
            1f, // รัศมีในการตรวจสอบ
            LayerMask.GetMask("Soil")
        );

        int yellowBlockCount = 0;
        foreach (var col in nearbyBlocks)
        {
            SoilBlock block = col.GetComponent<SoilBlock>();
            if (block != null && block.soilType == SoilType.NearFossil)
            {
                yellowBlockCount++;
            }
        }

        // ถ้าไม่มีบล็อกสีเหลืองเหลือ เริ่มการตก
        if (yellowBlockCount == 0 && !isFalling)
        {
            StartFalling();
        }
    }

    private void StartFalling()
    {
        isFalling = true;
        SetState(FossilState.Excavated);
        
        // เพิ่ม Rigidbody สำหรับการตก
        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.mass = 10f;
        rb.linearDamping = 1f;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        
        StartCoroutine(CheckGroundContact());
    }

    private IEnumerator CheckGroundContact()
    {
        float startY = transform.position.y;
        float previousY = startY;
        float stationaryTime = 0f;

        while (stationaryTime < 0.5f)
        {
            float currentY = transform.position.y;
            if (Mathf.Abs(currentY - previousY) < 0.01f)
            {
                stationaryTime += Time.deltaTime;
            }
            else
            {
                stationaryTime = 0f;
            }
            previousY = currentY;
            yield return null;
        }

        // เมื่อหยุดนิ่งแล้ว
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            Destroy(rb);
        }

        // จบเกมทันทีเมื่อฟอสซิลตกลงมาและหยุดนิ่ง
        GameManager.Instance.CompleteGame();
    }

    // Getter methods
    public FossilState GetCurrentState() => currentState;
    public float GetDurability() => currentDurability;
    public int GetFossilId() => fossilId;
}