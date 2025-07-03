using UnityEngine;
using System.Collections;

public class FossilSpawner : MonoBehaviour
{
    [Header("Fossil Spawning")]
    public FossilData fossilData; // ScriptableObject
    public Transform spawnPoint;  // จุด spawn กลาง (เช่น ใต้ดิน)
    public float clearRadius = 0.2f; // รัศมีที่ clear block รอบ fossil

    [Header("Spawn Effects")]
    public GameObject dustCloudPrefab;
    public GameObject rockDebrisPrefab;
    public float spawnEffectRadius = 1f;
    public float pushForce = 2f;

    void Start()
    {
        SpawnRandomFossil();
    }

    void SpawnRandomFossil()
    {
        if (!ValidateFossilData()) return;

        if (fossilData == null || fossilData.fossilPrefabs.Length == 0)
        {
            Debug.LogWarning("No Fossil Data Assigned!");
            return;
        }

        // เลือก fossil แบบสุ่ม
        int randomIndex = Random.Range(0, fossilData.fossilPrefabs.Length);
        GameObject prefab = fossilData.fossilPrefabs[randomIndex];
        string fossilName = fossilData.fossilNames[randomIndex];

        // สุ่มการหมุนเพื่อความสมจริง
        Quaternion randomRotation = Quaternion.Euler(
            0,
            Random.Range(0, 360),
            0
        );

        // Spawn fossil
        GameObject fossil = Instantiate(prefab, spawnPoint.position, randomRotation);
        fossil.name = "Fossil_" + fossilName;
        
        // กำหนด layer เป็น "Fossil"
        fossil.layer = LayerMask.NameToLayer("Fossil");

        Debug.Log("Spawned Fossil: " + fossilName);

        // หา SoilGenerator และสั่งให้ลบบล็อก
        var soilGenerator = FindObjectOfType<SoilGenerator>();
        if (soilGenerator != null)
        {
            soilGenerator.ClearBlocksInArea(fossil.transform.position, clearRadius);
            
            // เพิ่ม: บังคับให้บล็อกทั้งหมดเช็คการทับซ้อนใหม่
            var allBlocks = FindObjectsOfType<SoilBlock>();
            foreach (var block in allBlocks)
            {
                block.CheckNearbyFossils();
            }
        }
        else
        {
            Debug.LogWarning("SoilGenerator not found! Blocks won't be cleared.");
            ClearBlocksAround(fossil.transform.position, clearRadius);
        }

        // เรียกใช้เอฟเฟกต์การ spawn
        SpawnWithEffects(fossil);
    }
    
    bool ValidateFossilData()
    {
        if (fossilData == null || fossilData.fossilPrefabs.Length == 0)
        {
            Debug.LogWarning("No Fossil Data Assigned!");
            return false;
        }
        return true;
    }
    
    void ClearBlocksAround(Vector3 center, float radius)
    {
        // Use OverlapSphere instead of OverlapColliderNonAlloc
        Collider[] hitBlocks = new Collider[20]; // Adjust array size as needed
        int numColliders = Physics.OverlapSphereNonAlloc(center, radius, hitBlocks);

        int clearedBlocks = 0;
        for (int i = 0; i < numColliders; i++)
        {
            Collider col = hitBlocks[i];
            if (col != null)
            {
                SoilBlock block = col.GetComponent<SoilBlock>();
                if (block != null)
                {
                    clearedBlocks++;
                    Destroy(block.gameObject);
                }
            }
        }

        Debug.Log($"Cleared {clearedBlocks} blocks around fossil using OverlapSphere at {center}");
    }

    // เพิ่มฟังก์ชันใหม่เพื่อแสดง Gizmos สำหรับการ Debug
    void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            // แสดงรัศมีการ clear blocks
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // สีแดงโปร่งแสง
            Gizmos.DrawSphere(spawnPoint.position, clearRadius);
            
            // แสดงขอบเขตการ spawn
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, clearRadius);
            
            // แสดง Collider
            Collider fossilCollider = GetComponent<Collider>();
            if (fossilCollider != null)
            {
                // แสดงขอบเขต Collider
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                if (fossilCollider is BoxCollider boxCollider)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                }
                else if (fossilCollider is SphereCollider sphereCollider)
                {
                    Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
                }
                // สามารถเพิ่มการรองรับ Collider ประเภทอื่นๆ ได้ตามต้องการ
            }
        }
    }

    private void SpawnWithEffects(GameObject fossil)
    {
        // สร้างฝุ่นฟุ้ง
        if (dustCloudPrefab != null)
        {
            GameObject dust = Instantiate(dustCloudPrefab, fossil.transform.position, Quaternion.identity);
            Destroy(dust, 3f);
        }

        // สร้างเศษหินกระเด็น
        if (rockDebrisPrefab != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 randomPos = fossil.transform.position + Random.insideUnitSphere * 0.5f;
                GameObject debris = Instantiate(rockDebrisPrefab, randomPos, Random.rotation);
                if (debris.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.AddExplosionForce(pushForce, fossil.transform.position, spawnEffectRadius);
                }
                Destroy(debris, 2f);
            }
        }

        // ผลักดันบล็อกดินรอบๆ
        Collider[] nearbyBlocks = Physics.OverlapSphere(fossil.transform.position, spawnEffectRadius);
        foreach (var col in nearbyBlocks)
        {
            if (col.TryGetComponent<SoilBlock>(out var block))
            {
                // ทำให้ดินรอบๆ สั่น
                StartCoroutine(ShakeBlock(block.transform));
            }
        }
    }

    private IEnumerator ShakeBlock(Transform blockTransform)
    {
        // Store reference to the block component
        var block = blockTransform?.GetComponent<SoilBlock>();
        if (block == null) yield break;

        Vector3 originalPos = blockTransform.position;
        float elapsed = 0f;
        float duration = 0.5f;
        
        // Keep track if block is being destroyed
        bool isDestroyed = false;
        block.OnDestroyEvent += () => isDestroyed = true;
        
        while (elapsed < duration && !isDestroyed)
        {
            try
            {
                if (blockTransform != null)
                {
                    blockTransform.position = originalPos + Random.insideUnitSphere * 0.05f;
                }
                else
                {
                    break;
                }
            }
            catch (MissingReferenceException)
            {
                break;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        try
        {
            if (blockTransform != null && !isDestroyed)
            {
                blockTransform.position = originalPos;
            }
        }
        catch (MissingReferenceException)
        {
            // Ignore - block was destroyed
        }
    }
}
