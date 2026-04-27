using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    public LootItem[] lootPrefabs;
    public Transform spawnLineStart;
    public Transform spawnLineEnd;

    [Header("Spawn Timing")]
    public bool autoSpawn = true;
    public float spawnInterval = 0.75f;
    public int maxAlive = 40;
    public int initialBurstCount = 8;

    [Header("Height")]
    public float verticalJitter = 0.15f;

    private float spawnTimer;
    private int activeCount;

    private void Start()
    {
        SpawnInitialLoot();
    }

    private void Update()
    {
        if (!autoSpawn || lootPrefabs == null || lootPrefabs.Length == 0)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer < spawnInterval)
        {
            return;
        }

        spawnTimer = 0f;
        SpawnOne();
    }

    [ContextMenu("Spawn Initial Loot")]
    public void SpawnInitialLoot()
    {
        if (lootPrefabs == null || lootPrefabs.Length == 0)
        {
            return;
        }

        for (int i = 0; i < initialBurstCount; i++)
        {
            SpawnOne();
        }
    }

    private LootItem SpawnOne()
    {
        if (activeCount >= maxAlive)
        {
            return null;
        }

        LootItem prefab = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
        Vector3 spawnPos = GetSpawnPositionOnLine();
        LootItem item = Instantiate(prefab, spawnPos, Random.rotation);

        item.gameObject.AddComponent<SpawnedLootMarker>().owner = this;
        activeCount++;
        return item;
    }

    public void NotifyLootDespawned()
    {
        activeCount = Mathf.Max(0, activeCount - 1);
    }

    private Vector3 GetSpawnPositionOnLine()
    {
        Vector3 a = spawnLineStart != null ? spawnLineStart.position : transform.position;
        Vector3 b = spawnLineEnd != null ? spawnLineEnd.position : transform.position + Vector3.forward * 5f;
        Vector3 point = Vector3.Lerp(a, b, Random.value);
        point += Vector3.up * Random.Range(-verticalJitter, verticalJitter);
        return point;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 a = spawnLineStart != null ? spawnLineStart.position : transform.position;
        Vector3 b = spawnLineEnd != null ? spawnLineEnd.position : transform.position + Vector3.forward * 5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(a, 0.15f);
        Gizmos.DrawSphere(b, 0.15f);
    }
}

public class SpawnedLootMarker : MonoBehaviour
{
    public LootSpawner owner;

    private void OnDestroy()
    {
        if (owner != null)
        {
            owner.NotifyLootDespawned();
        }
    }
}
