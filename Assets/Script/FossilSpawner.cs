using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class FossilSpawner : MonoBehaviour
{
    [SerializeField]
    private FossilData fossilData;

    [SerializeField]
    private ObjectSpawner objectSpawner;

    [SerializeField]
    private Transform spawnPoint;

    private void Start()
    {
        if (objectSpawner == null)
        {
            objectSpawner = GetComponent<ObjectSpawner>();
        }
    }

    public void SpawnRandomFossil()
    {
        if (fossilData == null || fossilData.fossilPrefabs == null || fossilData.fossilPrefabs.Length == 0)
        {
            Debug.LogError("Fossil data not configured properly!");
            return;
        }

        // Configure object spawner with fossil prefabs
        objectSpawner.objectPrefabs.Clear();
        foreach (var fossil in fossilData.fossilPrefabs)
        {
            objectSpawner.objectPrefabs.Add(fossil);
        }

        // Randomize which fossil to spawn
        objectSpawner.RandomizeSpawnOption();

        // Spawn at the designated point
        if (spawnPoint != null)
        {
            objectSpawner.TrySpawnObject(spawnPoint.position, Vector3.up);
        }
        else
        {
            // If no spawn point set, spawn in front of the camera
            var camera = Camera.main;
            if (camera != null)
            {
                Vector3 spawnPosition = camera.transform.position + camera.transform.forward * 2f;
                objectSpawner.TrySpawnObject(spawnPosition, Vector3.up);
            }
        }
    }
}