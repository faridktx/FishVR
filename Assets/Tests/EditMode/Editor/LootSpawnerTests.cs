using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class LootSpawnerTests
{
    [Test]
    public void SpawnOne_AttachesRandomVisualAndHidesPlaceholderRenderer()
    {
        LootItem lootPrefab = CreateLootPrefab();
        GameObject visualPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualPrefab.name = "Visual_Can";

        LootSpawner spawner = new GameObject("Spawner").AddComponent<LootSpawner>();
        spawner.lootPrefabs = new[] { lootPrefab };
        spawner.lootVisualPrefabs = new[] { visualPrefab };

        LootItem spawned = null;
        try
        {
            spawned = InvokeSpawnOne(spawner);

            Assert.IsNotNull(spawned);
            Assert.IsFalse(spawned.GetComponent<Renderer>().enabled);
            Assert.AreEqual(1, spawned.transform.childCount);
            Assert.AreEqual("Visual_Can", spawned.transform.GetChild(0).name);
            Assert.IsTrue(spawned.transform.GetChild(0).GetComponent<Renderer>().enabled);
        }
        finally
        {
            if (spawned != null)
            {
                Object.DestroyImmediate(spawned.gameObject);
            }

            Object.DestroyImmediate(spawner.gameObject);
            Object.DestroyImmediate(lootPrefab.gameObject);
            Object.DestroyImmediate(visualPrefab);
        }
    }

    [Test]
    public void SpawnOne_KeepsPlaceholderVisibleWhenVisualHasNoRenderer()
    {
        LootItem lootPrefab = CreateLootPrefab();
        GameObject visualPrefab = new GameObject("Broken_Visual");

        LootSpawner spawner = new GameObject("Spawner").AddComponent<LootSpawner>();
        spawner.lootPrefabs = new[] { lootPrefab };
        spawner.lootVisualPrefabs = new[] { visualPrefab };

        LootItem spawned = null;
        try
        {
            spawned = InvokeSpawnOne(spawner);

            Assert.IsNotNull(spawned);
            Assert.IsTrue(spawned.GetComponent<Renderer>().enabled);
            Assert.AreEqual(0, spawned.transform.childCount);
        }
        finally
        {
            if (spawned != null)
            {
                Object.DestroyImmediate(spawned.gameObject);
            }

            Object.DestroyImmediate(spawner.gameObject);
            Object.DestroyImmediate(lootPrefab.gameObject);
            Object.DestroyImmediate(visualPrefab);
        }
    }

    [Test]
    public void SpawnOne_PreservesPrefabVisualScale()
    {
        LootItem lootPrefab = CreateLootPrefab();
        GameObject visualPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualPrefab.name = "Scaled_Visual";
        visualPrefab.transform.localScale = new Vector3(2f, 3f, 4f);

        LootSpawner spawner = new GameObject("Spawner").AddComponent<LootSpawner>();
        spawner.lootPrefabs = new[] { lootPrefab };
        spawner.lootVisualPrefabs = new[] { visualPrefab };
        spawner.visualLocalScale = new Vector3(1.5f, 1f, 0.5f);

        LootItem spawned = null;
        try
        {
            spawned = InvokeSpawnOne(spawner);

            Transform visual = spawned.transform.GetChild(0);
            Assert.AreEqual(new Vector3(3f, 3f, 2f), visual.localScale);
        }
        finally
        {
            if (spawned != null)
            {
                Object.DestroyImmediate(spawned.gameObject);
            }

            Object.DestroyImmediate(spawner.gameObject);
            Object.DestroyImmediate(lootPrefab.gameObject);
            Object.DestroyImmediate(visualPrefab);
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
