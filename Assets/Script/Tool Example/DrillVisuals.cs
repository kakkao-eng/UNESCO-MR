using UnityEngine;

public class DrillVisuals : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 360f;    // ความเร็วในการหมุน (องศาต่อวินาที)
    public Vector3 rotationAxis = Vector3.forward;  // แกนที่จะหมุน
    
    private DrillTool drillTool;
    
    void Start()
    {
        drillTool = GetComponentInParent<DrillTool>();
    }
    
    void Update()
    {
        // หมุนเมื่อกำลังเจาะ
        if (drillTool != null && drillTool.enabled)
        {
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
        }
    }
}