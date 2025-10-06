using UnityEngine;
using UnityEngine.XR;

public class GlueUnlimited : MonoBehaviour 
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

    private void Start()
    {
        // ดึง Input มือขวา
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        // ใช้กาวเมื่อกด Trigger มือขวา
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            // กำหนดตำแหน่งกาวตาม Offset
            Vector3 targetPoint = transform.position + transform.TransformDirection(glueOffset);
            UseGlue(targetPoint);
            lastUsedPoint = targetPoint;
        }
    }

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

        // เล่นเสียงถ้า Repair สำเร็จ
        if (repaired)
            SoundManager.PlaySound(SoundType.Glue);
    }

    private void OnDrawGizmos()
    {
        if (!showGlueLine) return;

        // วาดรัศมี Glue ตามตำแหน่งล่าสุดหรือ Offset
        Vector3 drawPosition = lastUsedPoint != Vector3.zero ? lastUsedPoint : transform.position + transform.TransformDirection(glueOffset);
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawSphere(drawPosition, glueRadius);
    }
}
