using UnityEngine;
using UnityEngine.XR;

public class ChiselTool : MonoBehaviour
{
    [Header("Tool Settings")]
    public float maxDistance = 2f;

    [Header("References")]
    public Transform toolTip;
    public ParticleSystem hitEffect;

    [Header("Layer Settings")]
    public LayerMask soilLayerMask;
    public LayerMask fossilLayerMask;

    [HideInInspector] public bool isAiming = false;
    [HideInInspector] public Vector3 aimPoint;

    public static ChiselTool activeChisel;

    private void Update()
    {
        UpdateChisel();
    }

    private void UpdateChisel()
    {
        if (Physics.Raycast(toolTip.position, toolTip.forward, out RaycastHit hit, maxDistance, soilLayerMask | fossilLayerMask))
        {
            isAiming = true;
            aimPoint = hit.point;
            activeChisel = this;
        }
        else
        {
            isAiming = false;
            if (activeChisel == this)
                activeChisel = null;
        }
    }

#if UNITY_EDITOR
    // แสดงเฉพาะใน Scene View
    private void OnDrawGizmos()
    {
        if (toolTip == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(toolTip.position, toolTip.position + toolTip.forward * maxDistance);
        Gizmos.DrawSphere(toolTip.position + toolTip.forward * maxDistance, 0.02f);

        if (isAiming)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(aimPoint, 0.2f);
        }
    }
#endif
}
