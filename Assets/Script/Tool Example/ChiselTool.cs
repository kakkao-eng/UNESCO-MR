using UnityEngine;
using ToolNamespace;

public class ChiselTool : MonoBehaviour
{
    public ToolController toolController;

    private void Awake()
    {
        if (toolController == null)
            toolController = GetComponentInParent<ToolController>();
    }

    /// <summary>
    /// ใช้สิ่วกับบล็อกดินเป้าหมาย
    /// </summary>
    public void Use(SoilBlock target)
    {
        if (toolController == null)
        {
            Debug.LogWarning("ChiselTool: ToolController not assigned.");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("ChiselTool: Target is null.");
            return;
        }

        // ตัวอย่าง: ทำความเสียหายกับบล็อก
        target.TakeDamage(1.0f);
        Debug.Log("ChiselTool: Used chisel on soil block.");
    }
}