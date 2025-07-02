using UnityEngine;
using System.Collections.Generic;

public class SoilGenerator : MonoBehaviour
{
    public GameObject soilPrefab;
    public int width = 10;
    public int length = 10;
    public int height = 10;
    public float blockSize = 0.1f;

    private Dictionary<Vector3Int, GameObject> soilBlocks = new Dictionary<Vector3Int, GameObject>();

    void Start()
    {
        GenerateSoil();
    }

    void GenerateSoil()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = new Vector3(
                        x * blockSize,
                        y * blockSize,
                        z * blockSize
                    );

                    GameObject block = Instantiate(soilPrefab, transform.position + position, 
                        Quaternion.identity, this.transform);
                    
                    // เก็บบล็อกลงใน Dictionary โดยใช้พิกัดเป็น key
                    Vector3Int gridPos = new Vector3Int(x, y, z);
                    soilBlocks[gridPos] = block;
                }
            }
        }
    }

    // เพิ่มฟังก์ชันสำหรับลบบล็อกในพื้นที่ที่กำหนด
    public void ClearBlocksInArea(Vector3 center, float radius)
    {
        // แปลงจากตำแหน่งโลกเป็นตำแหน่งในระบบกริด
        Vector3 localCenter = center - transform.position;
        Vector3Int gridCenter = new Vector3Int(
            Mathf.RoundToInt(localCenter.x / blockSize),
            Mathf.RoundToInt(localCenter.y / blockSize),
            Mathf.RoundToInt(localCenter.z / blockSize)
        );

        // คำนวณรัศมีในหน่วยบล็อก
        int blockRadius = Mathf.CeilToInt(radius / blockSize);
        int blocksCleared = 0;

        // วนลูปตรวจสอบบล็อกในพื้นที่
        for (int x = -blockRadius; x <= blockRadius; x++)
        {
            for (int y = -blockRadius; y <= blockRadius; y++)
            {
                for (int z = -blockRadius; z <= blockRadius; z++)
                {
                    Vector3Int checkPos = gridCenter + new Vector3Int(x, y, z);

                    // เช็คว่าตำแหน่งนี้มีบล็อกอยู่ไหม
                    if (soilBlocks.TryGetValue(checkPos, out GameObject block))
                    {
                        if (block != null)
                        {
                            float distance = Vector3.Distance(
                                block.transform.position,
                                center
                            );

                            if (distance <= radius)
                            {
                                Destroy(block);
                                soilBlocks.Remove(checkPos);
                                blocksCleared++;
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"Cleared {blocksCleared} blocks at position {center}");
    }

    void OnDrawGizmosSelected()
    {
        Vector3 size = new Vector3(width * blockSize, height * blockSize, length * blockSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + size/2, size);
    }
}
