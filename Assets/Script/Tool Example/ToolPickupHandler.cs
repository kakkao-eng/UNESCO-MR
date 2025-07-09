using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ToolPickupHandler : MonoBehaviour
{
    public ToolType toolType = ToolType.HammerChisel; // กำหนดชนิดเครื่องมือ
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // หา ToolController จาก Player หรือ XR Origin
        var toolController = FindObjectOfType<ToolController>();
        if (toolController != null)
        {
            // แจ้ง ToolController ว่าผู้เล่นหยิบเครื่องมือนี้
            toolController.SetCurrentTool(toolType, this.gameObject);
        }
    }
}
