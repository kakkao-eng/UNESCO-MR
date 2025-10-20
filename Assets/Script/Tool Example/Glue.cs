using UnityEngine;
using UnityEngine.XR;

public class Glue : MonoBehaviour 
{
    [Header("Glue Settings")]
    public float glueRadius = 0.3f;             // รัศมีของกาว
    public float repairStrength = 0.2f;         // ความแรงในการซ่อม
    public Vector3 glueOffset = new Vector3(0,0,0.2f); // ปรับตำแหน่งกาวได้ใน Inspector

    [Header("Effects")]
    public ParticleSystem glueEffect;           // เอฟเฟกต์กาวเวลาใช้งาน

    [Header("Debug Visualization")]
    public bool showGlueLine = true;            // แสดง Debug Line
    public Color glueLineColor = Color.cyan;

    private InputDevice rightHand;              // ตัวแทนมือขวา Pico
    private Vector3 lastUsedPoint;              // เก็บตำแหน่งกาวล่าสุด

    // 🔹 เพิ่มระบบเสียงแบบสมูท
    private bool isUsingGlue = false;           // ใช้ตรวจสอบว่ากำลังกด Trigger อยู่ไหม
    private float nextSoundTime = 0f;           // เวลาที่จะเล่นเสียงถัดไป
    private float soundCooldown = 0.4f;         // เวลาหน่วงเสียง (ป้องกันเสียงซ้ำถี่)

    private void Start()
    {
        // ดึง Input มือขวา
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        // ใช้กาวเมื่อกด Trigger มือขวา
        bool triggerPressed = false;
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
            triggerPressed = pressed;

        if (triggerPressed)
        {
            // 🔊 ถ้ายังไม่ได้เริ่มใช้งานกาว → เริ่มเล่นเสียงแบบต่อเนื่อง
            if (!isUsingGlue)
            {
                isUsingGlue = true;
                SoundManager.PlayLoop(SoundType.Glue);
            }

            // กำหนดตำแหน่งกาวตาม Offset
            Vector3 targetPoint = transform.position + transform.TransformDirection(glueOffset);
            UseGlue(targetPoint);
            lastUsedPoint = targetPoint;
        }
        else if (isUsingGlue)
        {
            // 🔇 หยุดเสียงเมื่อปล่อย Trigger
            isUsingGlue = false;
            SoundManager.StopSound();
        }
    }

    /// <summary>
    /// ฟังก์ชันหลักของกาว — ใช้สำหรับซ่อมฟอสซิล
    /// </summary>
    public void UseGlue(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, glueRadius);
        bool repaired = false;

        foreach (var hit in hits)
        {
            Fossil fossil = hit.GetComponent<Fossil>();
            if (fossil != null && fossil.GetCurrentState() == Fossil.FossilState.Damaged)
            {
                fossil.Repair(); // ซ่อมฟอสซิล
                repaired = true;

                // เอฟเฟกต์กาว
                if (glueEffect != null)
                {
                    var effect = Instantiate(glueEffect, hit.transform.position, Quaternion.identity);
                    effect.transform.rotation = Quaternion.LookRotation(Vector3.up);
                    Destroy(effect.gameObject, 2f);
                }
            }
        }

        // ✅ ถ้าซ่อมได้สำเร็จและครบ Cooldown → เล่นเสียง “แปะ” สั้นๆ เสริม
        if (repaired && Time.time >= nextSoundTime)
        {
            SoundManager.PlaySound(SoundType.Glue, 0.5f);
            nextSoundTime = Time.time + soundCooldown;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGlueLine) return;

        // วาดรัศมี Glue ตามตำแหน่งล่าสุดหรือ Offset
        Vector3 drawPosition = lastUsedPoint != Vector3.zero 
            ? lastUsedPoint 
            : transform.position + transform.TransformDirection(glueOffset);

        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawSphere(drawPosition, glueRadius);
    }
}
