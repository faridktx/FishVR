using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class LootSpawnerTests
{
    [Test]
    public void SpawnOne_InstantiatesAssignedLootPrefabWithVisibleRenderer()
    {
        LootItem lootPrefab = CreateLootPrefab();

        LootSpawner spawner = new GameObject("Spawner").AddComponent<LootSpawner>();
        spawner.lootPrefabs = new[] { lootPrefab };

        LootItem spawned = null;
        try
        {
            spawned = InvokeSpawnOne(spawner);

            Assert.IsNotNull(spawned);
            Assert.AreNotSame(lootPrefab, spawned);
            Assert.IsTrue(spawned.name.StartsWith(lootPrefab.name));
            Assert.IsTrue(spawned.GetComponent<Renderer>().enabled);
            Assert.IsNotNull(spawned.GetComponent<SpawnedLootMarker>());
        }
        finally
        {
            if (spawned != null)
            {
                Object.DestroyImmediate(spawned.gameObject);
            }

            Object.DestroyImmediate(spawner.gameObject);
            Object.DestroyImmediate(lootPrefab.gameObject);
        }
    }

    [Test]
    public void SpawnOne_ReturnsNullWhenSpawnerHasReachedMaxAlive()
    {
        LootItem lootPrefab = CreateLootPrefab();

        LootSpawner spawner = new GameObject("Spawner").AddComponent<LootSpawner>();
        spawner.lootPrefabs = new[] { lootPrefab };
        spawner.maxAlive = 1;

        LootItem spawned = null;
        try
        {
            spawned = InvokeSpawnOne(spawner);
            LootItem blockedSpawn = InvokeSpawnOne(spawner);

            Assert.IsNotNull(spawned);
            Assert.IsNull(blockedSpawn);
        }
        finally
        {
            if (spawned != null)
            {
                Object.DestroyImmediate(spawned.gameObject);
            }

            Object.DestroyImmediate(spawner.gameObject);
            Object.DestroyImmediate(lootPrefab.gameObject);
        }
    }

    [Test]
    public void SpawnOne_PreservesLootPrefabScale()
    {
        LootItem lootPrefab = CreateLootPrefab();
        lootPrefab.transform.localScale = new Vector3(2f, 3f, 4f);

        LootSpawner spawner = new GameObject("Spawner").AddComponent<LootSpawner>();
        spawner.lootPrefabs = new[] { lootPrefab };

        LootItem spawned = null;
        try
        {
            spawned = InvokeSpawnOne(spawner);

            Assert.AreEqual(new Vector3(2f, 3f, 4f), spawned.transform.localScale);
        }
        finally
        {
            if (spawned != null)
            {
                Object.DestroyImmediate(spawned.gameObject);
            }

            Object.DestroyImmediate(spawner.gameObject);
            Object.DestroyImmediate(lootPrefab.gameObject);
        }
    }

    private static LootItem CreateLootPrefab()
    {
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "LootPlaceholder";
        root.AddComponent<Rigidbody>();
        return root.AddComponent<LootItem>();
    }

    private static LootItem InvokeSpawnOne(LootSpawner spawner)
    {
        MethodInfo spawnOne = typeof(LootSpawner).GetMethod("SpawnOne", BindingFlags.Instance | BindingFlags.NonPublic);
        return (LootItem)spawnOne.Invoke(spawner, null);
    }
}
