using UnityEngine;
using ToolNamespace;

public class HammerChisel : MonoBehaviour
{
    public ToolController toolController;

    private void Awake()
    {
        if (toolController == null)
            toolController = GetComponentInParent<ToolController>();
    }

    /// <summary>
    /// ใช้ค้อนกับสิ่ว (ตัวอย่าง: อาจใช้กับบล็อกหรือฟังก์ชันอื่น)
    /// </summary>
    public void Use(SoilBlock target)
    {
        if (toolController == null)
        {
            Debug.LogWarning("HammerChisel: ToolController not assigned.");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("HammerChisel: Target is null.");
            return;
        }

        // ตัวอย่าง: ทำความเสียหายกับบล็อกมากกว่าสิ่วปกติ
        target.TakeDamage(2.0f);
        Debug.Log("HammerChisel: Used hammer and chisel on soil block.");
    }
}