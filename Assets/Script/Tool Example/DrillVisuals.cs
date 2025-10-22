using UnityEngine;

public class DrillVisuals : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 360f;    // ความเร็วในการหมุน (องศาต่อวินาที)
    public Vector3 rotationAxis = Vector3.up;  // เปลี่ยนเป็นหมุนรอบแกน Y
    
    private DrillTool drillTool;
    
    void Start()
    {
        // หา DrillTool ที่อยู่บนพาเรนต์
        drillTool = GetComponentInParent<DrillTool>();
    }
    
    void Update()
    {
        // หมุนเฉพาะตอนที่ DrillTool กำลังเจาะจริง ๆ
        if (drillTool != null && IsDrilling())
        {
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
            SoundManager.PlayLoop(SoundType.Drill);
        }
    }

    /// <summary>
    /// เช็คว่าตอนนี้สว่านกำลังทำงานอยู่ไหม
    /// </summary>
    private bool IsDrilling()
    {
        // ใช้ reflection ดูค่า private isDrilling ของ DrillTool
        var field = typeof(DrillTool).GetField("isDrilling", 
                      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null && drillTool != null)
        {
            return (bool)field.GetValue(drillTool);
        }
        return false;
    }
}
