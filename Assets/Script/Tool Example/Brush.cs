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
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            Vector3 brushPoint = transform.position + transform.TransformDirection(brushOffset);
            UseBrush(brushPoint);
            lastBrushPoint = brushPoint;
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

        // ✅ เล่นเสียงอย่างสมูท (มี cooldown)
        if (hitSomething && Time.time >= nextSoundTime)
        {
            SoundManager.PlaySound(SoundType.Brush);
            nextSoundTime = Time.time + soundCooldown;
        }
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
