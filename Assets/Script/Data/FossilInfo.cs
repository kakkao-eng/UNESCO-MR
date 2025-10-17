using UnityEngine;

[CreateAssetMenu(fileName = "NewFossilInfo", menuName = "Fossil Info/Create New Fossil Info")]
public class FossilInfo : ScriptableObject
{
    [Header("ข้อมูลพื้นฐาน")]
    public string fossilNameTH; // ชื่อไทย
    public string fossilNameEN; // ชื่ออังกฤษ

    [Header("ภาพประกอบ")]
    public Sprite fossilImage;  // รูปภาพของฟอสซิล

    [TextArea(3, 6)]
    [Header("ข้อมูลเพิ่มเติม")]
    public string fossilDescription; // ข้อมูลอธิบายเพิ่มเติม เช่น ถิ่นอาศัย, ลักษณะ, อายุ

    [Header("รหัสอ้างอิง")]
    public string fossilID; // ใช้ระบุฟอสซิลเฉพาะ เช่น "F01", "F02"
}
