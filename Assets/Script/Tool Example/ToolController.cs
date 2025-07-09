using UnityEngine;
using ToolNamespace;

// ผลลัพธ์การใช้เครื่องมือ
public enum ToolActionResult
{
    Success,            // สำเร็จ
    Fail,               // ล้มเหลว
    NeedHammer,         // ต้องใช้ค้อนร่วมด้วย
    NeedChisel,         // ต้องใช้สิ่วร่วมด้วย
    WrongTarget,        // เป้าหมายไม่ถูกต้อง
    AlreadyClean,       // ทำความสะอาดแล้ว
    AlreadyRepaired     // ซ่อมแซมแล้ว
}

// ตัวควบคุมการใช้เครื่องมือ
public class ToolController : MonoBehaviour
{
    public bool HasChisel { get; set; }   // มีสิ่วหรือไม่
    public bool HasHammer { get; set; }   // มีค้อนหรือไม่

    // 1. สิ่ว: ใช้ได้เฉพาะเมื่อมีค้อน และใช้กับดินที่ไม่ใช่ฟอสซิลเท่านั้น
    public ToolActionResult UseChisel(SoilBlock target)
    {
        if (target == null)
            return ToolActionResult.Fail;

        if (!HasHammer)
            return ToolActionResult.NeedHammer;
        if (target.soilType == SoilType.Fossil || target.soilType == SoilType.Damaged)
            return ToolActionResult.WrongTarget;

        target.TakeDamage(0.5f); // ปรับค่าความเสียหายตามต้องการ
        return ToolActionResult.Success;
    }

    // 2. ค้อน: ใช้ได้เฉพาะเมื่อมีสิ่ว ไม่สามารถใช้เดี่ยว ๆ ได้
    public ToolActionResult UseHammer()
    {
        if (!HasChisel)
            return ToolActionResult.NeedChisel;
        // ตรรกะของค้อนจะถูกจัดการใน UseChisel
        return ToolActionResult.Success;
    }

    // 3. แปรง: ทำความสะอาดดินใกล้ฟอสซิล ไม่ทำให้ฟอสซิลเสียหาย
    public ToolActionResult UseBrush(SoilBlock target)
    {
        if (target == null)
            return ToolActionResult.Fail;

        if (target.soilType == SoilType.NearFossil || target.soilType == SoilType.Fossil)
        {
            target.Brush();
            return ToolActionResult.Success;
        }
        return ToolActionResult.WrongTarget;
    }

    // 4. สว่านไฟฟ้า: ปลอดภัยสำหรับดินใกล้ฟอสซิล เศษดินน้อยกว่า แม่นยำกว่า
    public ToolActionResult UseDrill(SoilBlock target)
    {
        if (target == null)
            return ToolActionResult.Fail;

        if (target.soilType == SoilType.NearFossil || target.soilType == SoilType.Normal)
        {
            target.TakeDamage(0.3f); // ความเสียหายน้อยกว่า เศษดินน้อยกว่า
            // สามารถเพิ่มเอฟเฟกต์สว่านได้ที่นี่
            return ToolActionResult.Success;
        }
        return ToolActionResult.WrongTarget;
    }

    // 5. กาว: ซ่อมแซมฟอสซิลที่เสียหาย
    public ToolActionResult UseGlue(Fossil target)
    {
        if (target == null)
            return ToolActionResult.Fail;

        if (target.GetCurrentState() == Fossil.FossilState.Damaged)
        {
            target.Repair();
            return ToolActionResult.Success;
        }
        return ToolActionResult.AlreadyRepaired;
    }
}
