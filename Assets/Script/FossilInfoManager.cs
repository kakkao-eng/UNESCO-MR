using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class FossilInfoManager : MonoBehaviour
{
    [Header("Fossil Database (ScriptableObjects)")]
    public FossilInfo[] fossilDatabase; // เก็บข้อมูลฟอสซิลทั้งหมด

    [Header("UI Elements")]
    public Image fossilImageUI;
    public TMP_Text fossilNameTHUI;
    public TMP_Text fossilNameENUI;
    public TMP_Text fossilDescriptionUI;

    /// <summary>
    /// แสดงข้อมูลฟอสซิลบน UI โดยใช้ fossilID
    /// </summary>
    public void ShowFossilInfo(string fossilID)
    {
        var info = fossilDatabase.FirstOrDefault(f => f.fossilID == fossilID);

        if (info != null)
        {
            fossilNameTHUI.text = info.fossilNameTH;
            fossilNameENUI.text = info.fossilNameEN;
            fossilDescriptionUI.text = info.fossilDescription;
            fossilImageUI.sprite = info.fossilImage;
        }
        else
        {
            Debug.LogWarning($"ไม่พบข้อมูลฟอสซิลที่มีรหัส {fossilID}");
        }
    }
}
