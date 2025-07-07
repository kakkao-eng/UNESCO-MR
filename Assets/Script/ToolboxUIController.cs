using UnityEngine;

public class ToolboxUIController : MonoBehaviour
{
    public GameObject toolboxPanel; // Drag ToolboxPanel (World Space Canvas) มาวางใน Inspector

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // กด T เพื่อเปิด/ปิด Toolbox
            ToggleToolbox();
    }

    // เรียกฟังก์ชันนี้เมื่อกดปุ่ม/gesture/voice
    public void ToggleToolbox()
    {
        if (toolboxPanel != null)
            toolboxPanel.SetActive(!toolboxPanel.activeSelf);
    }
}
