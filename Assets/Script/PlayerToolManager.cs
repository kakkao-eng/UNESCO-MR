using UnityEngine;
using ToolNamespace;

public class PlayerToolManager : MonoBehaviour
{
    public ToolController toolController;
    public ToolType currentTool = ToolType.Chisel;

    void Start()
    {
        SelectChisel();
    }

    public void SelectChisel()
    {
        currentTool = ToolType.Chisel;
        if (toolController != null)
            toolController.HasChisel = true;
    }

    public void UseCurrentTool(SoilBlock target)
    {
        if (currentTool == ToolType.Chisel && toolController != null)
        {
            var result = toolController.UseChisel(target);
            Debug.Log("Chisel result: " + result);
        }
    }
}