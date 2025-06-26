using UnityEngine;
using UnityEngine.InputSystem; // Add this for new Input System

// Ensure FossilBehavior is defined and inherits from MonoBehaviour
public class FossilBehavior : MonoBehaviour
{
    public void CheckExposure()
    {
        // Implementation for checking fossil exposure
    }
}

public class MiningSystem : MonoBehaviour
{
    [Header("การตั้งค่าการขุด")]
    [SerializeField] private float digRadius = 0.5f; // รัศมีการขุด
    [SerializeField] private float digStrength = 0.2f; // ความแรงในการขุด
    [SerializeField] private float digSpeed = 0.1f; // ความเร็วในการขุด

    [Header("การตรวจจับ")]
    [SerializeField] private LayerMask groundLayer; // เลเยอร์ของพื้นที่ขุดได้
    [SerializeField] private float detectionRadius = 1f; // รัศมีการตรวจจับฟอสซิล

    private float nextDigTime;
    private MeshDeformer currentDeformer;

    private void Update()
    {
        if (Time.time < nextDigTime) return;

        // Using new Input System
        if (Mouse.current.leftButton.isPressed)
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 10f, groundLayer))
            {
                Dig(hit.point);
                CheckForFossils(hit.point);
                nextDigTime = Time.time + digSpeed;
            }
        }
    }

    private void Dig(Vector3 position)
    {
        // หา MeshDeformer ในพื้นที่ที่จะขุด
        var deformer = GetDeformerAtPosition(position);
        if (deformer != null)
        {
            deformer.DeformMeshAtPoint(position); // Updated to use the correct method
        }
    }

    private void CheckForFossils(Vector3 digPosition)
    {
        // ตรวจสอบฟอสซิลในรัศมีการขุด
        Collider[] colliders = Physics.OverlapSphere(digPosition, detectionRadius);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<FossilBehavior>(out var fossil)) // Use TryGetComponent to avoid allocation
            {
                fossil.CheckExposure();
            }
        }
    }

    private MeshDeformer GetDeformerAtPosition(Vector3 position)
    {
        // ค้นหา MeshDeformer ในพื้นที่
        Collider[] colliders = Physics.OverlapSphere(position, digRadius);
        foreach (var collider in colliders)
        {
            var deformer = collider.GetComponent<MeshDeformer>();
            if (deformer != null)
                return deformer;
        }
        return null;
    }
}