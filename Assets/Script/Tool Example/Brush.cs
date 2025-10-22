using UnityEngine;
using UnityEngine.XR;

public class Brush : MonoBehaviour
{
    [Header("Brush Settings")]
    public float brushRadius = 0.5f;
    public Vector3 brushOffset = new Vector3(0, 0, 0.2f);

    [Header("Effects")]
    public ParticleSystem dustEffect;

    [Header("Sound Settings")]
    public float soundCooldown = 0.3f; // หน่วงเวลาเสียง (วินาที)
    private float nextSoundTime = 0f;
    private bool isGrabbed = false;     // ✅ ต้องถือก่อนถึงใช้ได้
    private bool isBrushing = false;    // ✅ ตอนนี้กำลังกด Trigger อยู่ไหม
    private bool soundPlaying = false;  // ✅ ตรวจว่าเสียง Loop กำลังเล่นไหม

    [Header("Debug Visualization")]
    public bool showBrushGizmo = true;

    private InputDevice rightHand;
    private Vector3 lastBrushPoint;

    private void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        // ❌ ถ้ายังไม่ได้ถือเครื่องมือ หยุดทุกอย่าง
        if (!isGrabbed)
        {
            StopBrushing();
            return;
        }

        // ✅ ตรวจจับการกด Trigger
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            if (!isBrushing)
            {
                isBrushing = true;
                PlayBrushLoop();
            }

            Vector3 brushPoint = transform.position + transform.TransformDirection(brushOffset);
            UseBrush(brushPoint);
            lastBrushPoint = brushPoint;
        }
        else if (isBrushing)
        {
            StopBrushing();
        }
    }

    private void UseBrush(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, brushRadius);
        bool hitSomething = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("DirtChunk"))
            {
                Destroy(hit.gameObject);
                hitSomething = true;

                if (dustEffect != null)
                {
                    var dust = Instantiate(dustEffect, hit.transform.position, Quaternion.identity);
                    Destroy(dust.gameObject, 2f);
                }
            }
        }

        // ✅ เล่นเสียงเบา ๆ เป็นจังหวะเมื่อปัดโดน
        if (hitSomething && Time.time >= nextSoundTime)
        {
            SoundManager.PlaySound(SoundType.Brush, 0.8f);
            nextSoundTime = Time.time + soundCooldown;
        }
    }

    private void PlayBrushLoop()
    {
        if (!soundPlaying)
        {
            SoundManager.PlayLoop(SoundType.Brush);
            soundPlaying = true;
        }
    }

    private void StopBrushing()
    {
        isBrushing = false;

        if (soundPlaying)
        {
            // แก้ไข: ใช้ StopLoopSound เพื่อหยุดเสียงแปรงที่เล่นวนลูป
            SoundManager.StopLoopSound(); 
            soundPlaying = false;
        }
    }

    // ✅ เรียกจาก XR Grab Interactable
    public void OnGrabbed()
    {
        isGrabbed = true;
        Debug.Log("Brush grabbed!");
    }

    // ✅ เรียกจาก XR Grab Interactable
    public void OnReleased()
    {
        isGrabbed = false;
        StopBrushing();
        Debug.Log("Brush released!");
    }

    private void OnDrawGizmos()
    {
        if (!showBrushGizmo) return;

        Vector3 drawPosition = Application.isPlaying && lastBrushPoint != Vector3.zero
            ? lastBrushPoint
            : transform.position + transform.TransformDirection(brushOffset);

        Gizmos.color = new Color(1, 1, 0, 0.25f);
        Gizmos.DrawSphere(drawPosition, brushRadius);
    }
}
