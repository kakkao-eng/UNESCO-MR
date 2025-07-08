using UnityEngine;

namespace ToolNamespace
{
    // ประเภทของเครื่องมือ
    public enum ToolType
    {
        Chisel,         // สิ่ว
        Hammer,         // ค้อน
        Brush,          // แปรง
        ElectricDrill,  // สว่านไฟฟ้า
        Glue            // กาว
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
