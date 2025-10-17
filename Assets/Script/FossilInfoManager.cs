using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// จัดการข้อมูลฟอสซิลทั้งหมด และแสดงตอนจบเกม
/// </summary>
public class FossilInfoManager : MonoBehaviour
{
    [Header("ฐานข้อมูลฟอสซิลทั้งหมด")]
    public FossilInfo[] fossilDatabase; // ใส่ข้อมูลทั้งหมดใน Inspector

    [Header("UI สำหรับแสดงข้อมูลตอนจบเกม")]
    public Image fossilImageUI;
    public TMP_Text fossilNameText;
    public TMP_Text fossilDescriptionText;

    [Header("หน้าต่างแสดงข้อมูล")]
    public GameObject infoPanel;

    /// <summary>
    /// เรียกข้อมูลฟอสซิลตามรหัส เช่น "F01"
    /// </summary>
    public void ShowFossilInfo(string fossilID)
    {
        FossilInfo info = GetFossilInfoByID(fossilID);

        if (info == null)
        {
            Debug.LogWarning($"❌ Fossil ID '{fossilID}' not found in database!");
            return;
        }

        // แสดงข้อมูลบน UI
        if (fossilImageUI != null) fossilImageUI.sprite = info.fossilImage;
        if (fossilNameText != null) fossilNameText.text = $"{info.fossilNameTH} ({info.fossilNameEN})";
        if (fossilDescriptionText != null) fossilDescriptionText.text = info.fossilDescription;

        if (infoPanel != null) infoPanel.SetActive(true);
    }

    /// <summary>
    /// คืนค่า ScriptableObject ที่ตรงกับ ID
    /// </summary>
    private FossilInfo GetFossilInfoByID(string fossilID)
    {
        foreach (var fossil in fossilDatabase)
        {
            if (fossil.fossilID == fossilID)
                return fossil;
        }
        return null;
    }
}
