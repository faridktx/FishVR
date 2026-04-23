using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    public LootItem[] lootPrefabs;
    public GameObject[] lootVisualPrefabs;
    public Transform spawnLineStart;
    public Transform spawnLineEnd;

    [Header("Visuals")]
    public bool hidePlaceholderVisuals = true;
    public Vector3 visualLocalPosition;
    public Vector3 visualLocalEulerAngles;
    public Vector3 visualLocalScale = Vector3.one;

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
        ApplyRandomVisual(item);
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

    private void ApplyRandomVisual(LootItem item)
    {
        if (item == null || lootVisualPrefabs == null || lootVisualPrefabs.Length == 0)
        {
            return;
        }

        GameObject visualPrefab = lootVisualPrefabs[Random.Range(0, lootVisualPrefabs.Length)];
        if (visualPrefab == null)
        {
            return;
        }

        Renderer[] placeholderRenderers = hidePlaceholderVisuals ? item.GetComponentsInChildren<Renderer>() : null;
        GameObject visual;
        try
        {
            visual = Instantiate(visualPrefab, item.transform);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Loot visual '{visualPrefab.name}' could not be spawned, leaving placeholder visible. {exception.Message}", this);
            return;
        }

        if (!HasEnabledRenderer(visual))
        {
            DestroyCreatedVisual(visual);
            Debug.LogWarning($"Loot visual '{visualPrefab.name}' has no enabled renderers, leaving placeholder visible.", this);
            return;
        }

        visual.name = visualPrefab.name;
        Transform visualTransform = visual.transform;
        Vector3 prefabLocalScale = visualTransform.localScale;
        visualTransform.localPosition = visualLocalPosition;
        visualTransform.localRotation = Quaternion.Euler(visualLocalEulerAngles);
        visualTransform.localScale = Vector3.Scale(prefabLocalScale, visualLocalScale);

        if (hidePlaceholderVisuals)
        {
            SetRenderersEnabled(placeholderRenderers, false);
        }
    }

    private static bool HasEnabledRenderer(GameObject visual)
    {
        if (visual == null)
        {
            return false;
        }

        Renderer[] visualRenderers = visual.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < visualRenderers.Length; i++)
        {
            if (visualRenderers[i].enabled)
            {
                return true;
            }
        }

        return false;
    }

    private static void SetRenderersEnabled(Renderer[] renderers, bool enabled)
    {
        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = enabled;
            }
        }
    }

    private static void DestroyCreatedVisual(GameObject visual)
    {
        if (Application.isPlaying)
        {
            Destroy(visual);
        }
        else
        {
            DestroyImmediate(visual);
        }
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
