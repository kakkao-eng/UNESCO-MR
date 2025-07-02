using UnityEngine;

public class HammerChisel : MonoBehaviour
{
    //(สิ่ว+ฆ้อน)
    public float damage = 0.4f;
    public float range = 2f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // VR อาจใช้ trigger event
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
            {
                SoilBlock block = hit.collider.GetComponent<SoilBlock>();
                if (block != null)
                {
                    block.TakeDamage(damage);
                }
            }
        }
    }
}
