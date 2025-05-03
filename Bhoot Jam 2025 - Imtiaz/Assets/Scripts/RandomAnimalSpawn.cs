using UnityEngine;
using UnityEngine.AI;

public class RandomAnimalSpawn : MonoBehaviour
{
    public GameObject[] animalPrefabs; // Array of animal prefabs
    public int numberOfAnimals = 10;
    public float spawnRange = 20f;
    public float noSpawnInnerRadius = 5f;
    public Transform centerPoint;

    public float navMeshCheckRadius = 2f;

    void Start()
    {
        for (int i = 0; i < numberOfAnimals; i++)
        {
            Vector3 spawnPos = GetRandomNavMeshPosition();
            if (spawnPos != Vector3.zero)
            {
                GameObject randomAnimal = GetRandomAnimalPrefab();
                Instantiate(randomAnimal, spawnPos, Quaternion.identity);
            }
        }
    }

    Vector3 GetRandomNavMeshPosition()
    {
        for (int attempts = 0; attempts < 20; attempts++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRange;
            randomOffset.y = 0f;
            Vector3 candidatePos = centerPoint.position + randomOffset;

            float distance = Vector3.Distance(candidatePos, centerPoint.position);
            if (distance < noSpawnInnerRadius)
                continue;

            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, navMeshCheckRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        Debug.LogWarning("Failed to find valid NavMesh position.");
        return Vector3.zero;
    }

    GameObject GetRandomAnimalPrefab()
    {
        if (animalPrefabs.Length == 0)
        {
            Debug.LogError("No animal prefabs assigned.");
            return null;
        }

        int index = Random.Range(0, animalPrefabs.Length);
        return animalPrefabs[index];
    }

    void OnDrawGizmosSelected()
    {
        if (centerPoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPoint.position, spawnRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPoint.position, noSpawnInnerRadius);
    }
}
