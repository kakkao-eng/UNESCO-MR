using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshDeformer : MonoBehaviour
{
    [Header("Deformation Settings")]
    [SerializeField] private float deformStrength = 0.1f;
    [SerializeField] private float deformRadius = 0.5f;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] originalVertices;
    private MeshCollider meshCollider;

    void Start()
    {
        // คัดลอก Mesh เพื่อให้สามารถแก้ไขได้
        var meshFilter = GetComponent<MeshFilter>();
        mesh = Instantiate(meshFilter.sharedMesh);
        meshFilter.mesh = mesh;

        // เก็บข้อมูลจุดยอด
        vertices = mesh.vertices;
        originalVertices = mesh.vertices;

        meshCollider = GetComponent<MeshCollider>();
    }

    public void DeformMeshAtPoint(Vector3 worldPoint)
    {
        // แปลงพิกัดโลกเป็นพิกัดท้องถิ่น
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

        bool hasChanged = false;

        // วนลูปทุกจุดยอดและปรับแต่งตามระยะห่าง
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(vertices[i], localPoint);
            if (distance < deformRadius)
            {
                float effect = 1.0f - (distance / deformRadius);
                vertices[i] += Vector3.down * effect * deformStrength;
                hasChanged = true;
            }   
        }

        // อัพเดท Mesh ถ้ามีการเปลี่ยนแปลง
        if (hasChanged)
        {
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // อัพเดท Collider
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    public void ResetMesh()
    {
        // คืนค่า Mesh กลับไปยังสถานะเริ่มต้น
        vertices = (Vector3[])originalVertices.Clone();
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    private void Dig(Vector3 position)
    {
        var deformer = GetDeformerAtPosition(position);
        if (deformer != null)
        {
            deformer.DeformMeshAtPoint(position); // แก้จาก DeformMesh เป็น DeformMeshAtPoint
        }
    }
    private MeshDeformer GetDeformerAtPosition(Vector3 position)
    {
        // Assuming the deformer is attached to the same GameObject
        // or nearby GameObjects, you can use a Physics.OverlapSphere to find it.
        Collider[] colliders = Physics.OverlapSphere(position, deformRadius);
        foreach (var collider in colliders)
        {
            MeshDeformer deformer = collider.GetComponent<MeshDeformer>();
            if (deformer != null)
            {
                return deformer;
            }
        }
        return null; // Return null if no deformer is found
    }
}