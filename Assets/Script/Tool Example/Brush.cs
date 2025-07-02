using UnityEngine;

public class Brush : MonoBehaviour
{
    public float range = 2f;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // คลิกขวา
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
            {
                SoilBlock block = hit.collider.GetComponent<SoilBlock>();
                if (block != null)
                {
                    block.Brush();
                }
            }
        }
    }
}