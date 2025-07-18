using Script.Tool_Example;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChiselAndHammerTool : MonoBehaviour
{
    public enum ToolMode
    {
        Chisel,
        Hammer
    }

    [Header("Tool Settings")]
    public ToolMode currentMode;
    public float maxDistance = 2f;
    public float hitRadius = 0.2f;
    public float hitForce = 20f;
    public float maxHitDistance = 0.5f;

    [Header("References")]
    public Transform toolTip;
    public SoilGenerator soilGenerator;
    public ParticleSystem hitEffect;
    public AudioSource audioSource;
    public AudioClip hitSound;

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;
    public LayerMask fossilLayerMask;

    // สำหรับสิ่ว
    private bool isAiming = false;
    private Vector3 aimPoint;
    private static ChiselAndHammerTool activeChisel; // เก็บสิ่วที่กำลังเล็งอยู่

    private void Start()
    {
        if (soilGenerator == null)
            soilGenerator = FindObjectOfType<SoilGenerator>();
    }

    private void Update()
    {
        if (currentMode == ToolMode.Chisel)
        {
            UpdateChisel();
        }
        else if (currentMode == ToolMode.Hammer)
        {
            UpdateHammer();
        }
    }

    private void UpdateChisel()
    {
        RaycastHit hit;
        if (Physics.Raycast(toolTip.position, toolTip.forward, out hit, maxDistance, soilLayerMask | fossilLayerMask))
        {
            isAiming = true;
            aimPoint = hit.point;
            activeChisel = this;
            
            // อาจจะเพิ่ม Visual feedback ที่นี่
            Debug.DrawLine(toolTip.position, hit.point, Color.yellow);
        }
        else
        {
            isAiming = false;
            if (activeChisel == this)
                activeChisel = null;
        }
    }

    private void UpdateHammer()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && activeChisel != null)
        {
            float distanceToChisel = Vector3.Distance(transform.position, activeChisel.toolTip.position);
            if (distanceToChisel <= maxHitDistance)
            {
                HitWithHammer();
            }
        }
    }

    private void HitWithHammer()
    {
        if (!isAiming || activeChisel == null) return;

        // เอฟเฟกต์การตี
        if (hitEffect != null)
        {
            hitEffect.transform.position = aimPoint;
            hitEffect.Play();
        }

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // ขุดบล็อก
        RaycastHit[] hits = Physics.SphereCastAll(
            activeChisel.toolTip.position, 
            hitRadius, 
            activeChisel.toolTip.forward, 
            maxDistance, 
            soilLayerMask | fossilLayerMask
        );

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<SoilBlock>(out var soilBlock))
            {
                soilBlock.TakeDamage(hitForce);
            }
            else if (hit.collider.TryGetComponent<Fossil>(out var fossil))
            {
                fossil.TakeDamage(hitForce, ToolType.Chisel);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (isAiming)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(aimPoint, hitRadius);
        }
    }
}