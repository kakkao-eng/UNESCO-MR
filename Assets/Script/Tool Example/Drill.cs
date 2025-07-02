using UnityEngine;

public class Drill : MonoBehaviour
{
    public float damage = 0.1f;
    public float range = 2f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
            {
                SoilBlock block = hit.collider.GetComponent<SoilBlock>();
                if (block != null && block.soilType != SoilType.Fossil)
                {
                    block.TakeDamage(damage);
                }
            }
        }
    }
}
