using UnityEngine;
using UnityEngine.XR;

public class Brush : MonoBehaviour 
{
    [Header("Brush Settings")]
    public float brushRadius = 0.5f;                // รัศมีการทำงานของแปรง
    public float brushStrength = 0.3f;              // ความแรงของแปรง
    public Vector3 brushOffset = new Vector3(0, 0, 0.2f); // ปรับตำแหน่งรัศมีแปรงเองได้

    [Header("Effects")]
    public ParticleSystem dustEffect;                // เอฟเฟกต์ฝุ่นเวลาแปรงดิน

    [Header("Debug Visualization")]
    public bool showBrushGizmo = true;               // เปิด/ปิดการแสดงรัศมีแปรง

    private InputDevice rightHand;                   // ตัวแทนมือขวา Pico
    private Vector3 lastBrushPoint;                  // จุดแปรงล่าสุด (ใช้วาด Gizmos)

    private void Start()
    {
        // ดึง Input มือขวา
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        // ตรวจสอบ Trigger มือขวา
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            // คำนวณตำแหน่งจริงของจุดศูนย์กลางแปรง
            Vector3 brushPoint = transform.position + transform.TransformDirection(brushOffset);
            UseBrush(brushPoint);
            lastBrushPoint = brushPoint;
        }
    }

    public void UseBrush(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, brushRadius);
        bool hitSomething = false;

        foreach (var hit in hits)
        {
            SoilBlock soil = hit.GetComponent<SoilBlock>();
            if (soil != null && soil.soilType == SoilType.NearFossil)
            {
                soil.Brush(); // ปัดดินใกล้ Fossil
                hitSomething = true;

                // สร้างเอฟเฟกต์ฝุ่น
                if (dustEffect != null)
                {
                    var dust = Instantiate(dustEffect, soil.transform.position, Quaternion.identity);
                    Destroy(dust.gameObject, 2f);
                }
            }
        }

        // เล่นเสียงถ้าเจอดิน
        if (hitSomething)
            SoundManager.PlaySound(SoundType.Brush);
    }

    private void OnDrawGizmos()
    {
        if (!showBrushGizmo) return;

        // จุดศูนย์กลางของรัศมีแปรง (ใช้ Offset)
        Vector3 drawPosition = Application.isPlaying && lastBrushPoint != Vector3.zero
            ? lastBrushPoint
            : transform.position + transform.TransformDirection(brushOffset);

        // วาดรัศมีของแปรง
        Gizmos.color = new Color(1, 1, 0, 0.25f);
        Gizmos.DrawSphere(drawPosition, brushRadius);
    }
}
