using UnityEngine;

public class Glue : MonoBehaviour
{
    public float range = 2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
            {
                SoilBlock block = hit.collider.GetComponent<SoilBlock>();
                if (block != null)
                {
                    block.Repair();
                }
            }
        }
    }
}