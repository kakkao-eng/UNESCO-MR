using UnityEngine;

namespace ToolNamespace
{
    // ประเภทของเครื่องมือ
    public enum ToolType
    {
        Chisel,
        Hammer,
        Brush,
        ElectricDrill,
        Glue
    }

    // ข้อมูล ScriptableObject สำหรับเครื่องมือแต่ละชนิด
    [CreateAssetMenu(fileName = "ToolData", menuName = "Scriptable Objects/ToolData")]
    public class ToolData : ScriptableObject
    {
        public ToolType toolType;         // ประเภทของเครื่องมือ
        [TextArea]
        public string description;        // คำอธิบาย
        [TextArea]
        public string usageNote;          // หมายเหตุการใช้งาน
        public Sprite icon;               // ไอคอน
    }
}
