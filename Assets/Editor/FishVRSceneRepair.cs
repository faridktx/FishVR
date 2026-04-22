using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class FishVRSceneRepair
{
    private const string VisualPrefabFolder = "Assets/Prefabs/Game/LootVisuals";
    private const string JunkMaterialFolder = "Assets/Props/Junk/Materials";

    private static readonly (string assetPath, string prefabName, float localScale)[] LootVisualModels =
    {
        ("Assets/Props/Junk/Can/can_crushed_blue_lp.fbx", "LootVisual_Can_Blue", 8f),
        ("Assets/Props/Junk/Can/can_crushed_red_lp.fbx", "LootVisual_Can_Red", 8f),
        ("Assets/Props/Junk/Can/can_crushed_green_lp.fbx", "LootVisual_Can_Green", 8f),
        ("Assets/Props/Junk/Tire/tire_junk_lp.fbx", "LootVisual_Tire", 80f),
        ("Assets/Props/Junk/Microwave/microwave_junk_v2_lp.fbx", "LootVisual_Microwave", 2.4f),
        ("Assets/Props/Junk/Toaster/toaster_junk_lp.fbx", "LootVisual_Toaster", 3f),
    };

    private static readonly string[] LootBasePrefabPaths =
    {
        "Assets/Prefabs/Game/Loot_1.prefab",
        "Assets/Prefabs/Game/Loot_2.prefab",
        "Assets/Prefabs/Game/Loot_bomb.prefab",
    };

    [MenuItem("Tools/FishVR/Rebuild Loot Visuals And Rewire Scene")]
    public static void RebuildLootVisualsAndRewireScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Exit Play Mode before rebuilding loot visuals.");
            return;
        }

        Directory.CreateDirectory(VisualPrefabFolder);

        List<GameObject> visualPrefabs = new List<GameObject>();
        for (int i = 0; i < LootVisualModels.Length; i++)
        {
            GameObject visualPrefab = BuildVisualPrefab(LootVisualModels[i].assetPath, LootVisualModels[i].prefabName, LootVisualModels[i].localScale);
            if (visualPrefab != null)
            {
                visualPrefabs.Add(visualPrefab);
            }
        }

        LootItem[] lootPrefabs = LoadLootPrefabs();
        LootSpawner[] spawners = Object.FindObjectsByType<LootSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < spawners.Length; i++)
        {
            Undo.RecordObject(spawners[i], "Assign loot visual prefabs");
            spawners[i].lootPrefabs = lootPrefabs;
            spawners[i].lootVisualPrefabs = visualPrefabs.ToArray();
            spawners[i].hidePlaceholderVisuals = true;
            spawners[i].visualLocalPosition = Vector3.zero;
            spawners[i].visualLocalEulerAngles = Vector3.zero;
            spawners[i].visualLocalScale = Vector3.one;
            EditorUtility.SetDirty(spawners[i]);
            EditorSceneManager.MarkSceneDirty(spawners[i].gameObject.scene);
        }

        int deactivatedTemplates = DeactivateSceneTemplateLoot(spawners);

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"FishVRSceneRepair rebuilt {visualPrefabs.Count} loot visual prefabs, assigned {lootPrefabs.Length} base prefab assets to {spawners.Length} LootSpawner object(s), and deactivated {deactivatedTemplates} scene template loot object(s).");
    }

    [MenuItem("Tools/FishVR/Reimport Pier FBX")]
    public static void ReimportPierFbx()
    {
        AssetDatabase.ImportAsset("Assets/Props/Environment/Pier/pier_ruined_fishing_lp.fbx", ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        Debug.Log("FishVRSceneRepair reimported Assets/Props/Environment/Pier/pier_ruined_fishing_lp.fbx.");
    }

    [MenuItem("Tools/FishVR/Log Runtime Loot Visual State")]
    public static void LogRuntimeLootVisualState()
    {
        LootItem[] lootItems = Object.FindObjectsByType<LootItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int activeSceneTemplates = 0;
        int spawned = 0;
        int spawnedWithVisual = 0;
        int spawnedWithVisiblePlaceholder = 0;

        for (int i = 0; i < lootItems.Length; i++)
        {
            bool isRuntimeSpawn = IsRuntimeSpawn(lootItems[i]);
            if (!isRuntimeSpawn && IsSceneTemplateLoot(lootItems[i]) && lootItems[i].gameObject.activeInHierarchy)
            {
                activeSceneTemplates++;
            }

            if (!isRuntimeSpawn)
            {
                continue;
            }

            spawned++;
            if (HasEnabledChildRenderer(lootItems[i].transform))
            {
                spawnedWithVisual++;
            }

            Renderer placeholderRenderer = lootItems[i].GetComponent<Renderer>();
            if (placeholderRenderer != null && placeholderRenderer.enabled)
            {
                spawnedWithVisiblePlaceholder++;
            }
        }

        Debug.Log($"FishVRSceneRepair runtime loot state: {spawned} spawned loot item(s), {spawnedWithVisual} with visible child visual(s), {spawnedWithVisiblePlaceholder} with visible root placeholder renderer(s), {activeSceneTemplates} active scene template placeholder(s).");
    }

    [MenuItem("Tools/FishVR/Apply Loot Material Colors")]
    public static void ApplyLootMaterialColors()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Exit Play Mode before applying loot material colors.");
            return;
        }

        Directory.CreateDirectory(JunkMaterialFolder);

        int coloredSlots = 0;
        for (int i = 0; i < LootVisualModels.Length; i++)
        {
            string prefabPath = $"{VisualPrefabFolder}/{LootVisualModels[i].prefabName}.prefab";
            coloredSlots += ApplyMaterialsToPrefab(prefabPath, LootVisualModels[i].prefabName);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"FishVRSceneRepair applied colored loot materials to {coloredSlots} renderer slot(s).");
    }

    [MenuItem("Tools/FishVR/Assert Loot And Pier Setup")]
    public static void AssertLootAndPierSetup()
    {
        if (EditorApplication.isPlaying)
        {
            throw new System.InvalidOperationException("Exit Play Mode before asserting loot and pier setup.");
        }

        AssertVisualPrefabScales();
        AssertVisualPrefabMaterials();
        AssertPierSceneState();
        AssertSceneSpawner();
        AssertSpawnedVisualReplacesPlaceholderAndPreservesScale();
        Debug.Log("FishVRSceneRepair setup check passed: pier is using the restored mesh, loot visuals are colored/wired, placeholders hidden, prefab scale preserved.");
    }

    private static GameObject BuildVisualPrefab(string modelPath, string prefabName, float localScale)
    {
        AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);

        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (model == null)
        {
            Debug.LogError($"Could not load loot visual model at {modelPath}.");
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        if (instance == null)
        {
            instance = Object.Instantiate(model);
        }

        instance.name = prefabName;
        instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        instance.transform.localScale = Vector3.one * localScale;

        if (instance.GetComponentsInChildren<Renderer>(true).Length == 0)
        {
            Debug.LogError($"Loot visual model at {modelPath} has no renderers.");
            Object.DestroyImmediate(instance);
            return null;
        }

        string prefabPath = $"{VisualPrefabFolder}/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);

        if (prefab == null)
        {
            Debug.LogError($"Could not save loot visual prefab at {prefabPath}.");
            return null;
        }

        ApplyMaterialsToPrefab(prefabPath, prefabName);
        return prefab;
    }

    private static int ApplyMaterialsToPrefab(string prefabPath, string prefabName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Could not load loot visual prefab at {prefabPath}.");
            return 0;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
        int coloredSlots = 0;
        try
        {
            Renderer[] renderers = contents.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] existingMaterials = renderers[i].sharedMaterials;
                Material[] coloredMaterials = new Material[existingMaterials.Length];
                for (int j = 0; j < existingMaterials.Length; j++)
                {
                    coloredMaterials[j] = GetLootMaterial(prefabName, j);
                    coloredSlots++;
                }

                renderers[i].sharedMaterials = coloredMaterials;
                EditorUtility.SetDirty(renderers[i]);
            }

            PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        return coloredSlots;
    }

    private static void AssertVisualPrefabScales()
    {
        for (int i = 0; i < LootVisualModels.Length; i++)
        {
            string prefabPath = $"{VisualPrefabFolder}/{LootVisualModels[i].prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            AssertCondition(prefab != null, $"Missing loot visual prefab at {prefabPath}.");
            AssertUniformScale(prefab.transform.localScale, LootVisualModels[i].localScale, prefabPath);
        }
    }

    private static void AssertVisualPrefabMaterials()
    {
        for (int i = 0; i < LootVisualModels.Length; i++)
        {
            string prefabPath = $"{VisualPrefabFolder}/{LootVisualModels[i].prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            AssertCondition(prefab != null, $"Missing loot visual prefab at {prefabPath}.");

            bool hasDistinctiveColor = false;
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j < renderers.Length; j++)
            {
                Material[] materials = renderers[j].sharedMaterials;
                for (int k = 0; k < materials.Length; k++)
                {
                    AssertCondition(materials[k] != null, $"{prefabPath} has an empty material slot.");
                    Color color = GetMaterialColor(materials[k]);
                    if (IsDistinctiveColor(color))
                    {
                        hasDistinctiveColor = true;
                    }
                }
            }

            AssertCondition(hasDistinctiveColor, $"{prefabPath} has no distinctive colored material.");
        }
    }

    private static void AssertPierSceneState()
    {
        GameObject previewGround = FindSceneGameObject("Pier_Preview_Ground");
        AssertCondition(previewGround == null || (!previewGround.activeSelf && !previewGround.activeInHierarchy), "Pier_Preview_Ground must not be active.");

        GameObject pier = FindSceneGameObject("Pier_Ruined_Fishing_LP");
        if (pier == null)
        {
            pier = FindSceneGameObject("pier_ruined_fishing_lp");
        }

        AssertCondition(pier != null && pier.activeInHierarchy, "Pier_Ruined_Fishing_LP must be active in the scene.");
    }

    private static void AssertSceneSpawner()
    {
        LootSpawner[] spawners = Object.FindObjectsByType<LootSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AssertCondition(spawners.Length > 0, "Scene has no LootSpawner.");

        LootSpawner sceneSpawner = null;
        for (int i = 0; i < spawners.Length; i++)
        {
            if (spawners[i].gameObject.scene.IsValid())
            {
                sceneSpawner = spawners[i];
                break;
            }
        }

        AssertCondition(sceneSpawner != null, "No scene LootSpawner found.");
        AssertCondition(sceneSpawner.hidePlaceholderVisuals, "LootSpawner must hide placeholder visuals.");
        AssertCondition(sceneSpawner.visualLocalScale == Vector3.one, "LootSpawner visualLocalScale should remain a neutral multiplier.");

        for (int i = 0; i < LootVisualModels.Length; i++)
        {
            AssertCondition(HasAssignedVisual(sceneSpawner, LootVisualModels[i].prefabName), $"LootSpawner is missing visual prefab {LootVisualModels[i].prefabName}.");
        }
    }

    private static void AssertSpawnedVisualReplacesPlaceholderAndPreservesScale()
    {
        GameObject lootRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lootRoot.name = "AssertLootPlaceholder";
        lootRoot.AddComponent<Rigidbody>();
        LootItem lootPrefab = lootRoot.AddComponent<LootItem>();

        GameObject visualPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualPrefab.name = "AssertScaledVisual";
        visualPrefab.transform.localScale = new Vector3(2f, 3f, 4f);

        GameObject spawnerObject = new GameObject("AssertSpawner");
        LootSpawner spawner = spawnerObject.AddComponent<LootSpawner>();
        spawner.lootPrefabs = new[] { lootPrefab };
        spawner.lootVisualPrefabs = new[] { visualPrefab };
        spawner.visualLocalScale = new Vector3(1.5f, 1f, 0.5f);

        LootItem spawned = null;
        try
        {
            System.Reflection.MethodInfo spawnOne = typeof(LootSpawner).GetMethod("SpawnOne", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            spawned = (LootItem)spawnOne.Invoke(spawner, null);

            AssertCondition(spawned != null, "SpawnOne returned null.");
            Renderer placeholderRenderer = spawned.GetComponent<Renderer>();
            AssertCondition(placeholderRenderer != null && !placeholderRenderer.enabled, "Spawned root placeholder renderer is still visible.");
            AssertCondition(spawned.transform.childCount == 1, "Spawned loot should have exactly one visual child.");

            Transform visual = spawned.transform.GetChild(0);
            AssertCondition(visual.name == visualPrefab.name, "Spawned visual child has the wrong prefab name.");
            AssertCondition(visual.GetComponent<Renderer>() != null && visual.GetComponent<Renderer>().enabled, "Spawned visual child renderer is not enabled.");
            AssertVectorScale(visual.localScale, new Vector3(3f, 3f, 2f), "Spawned visual did not preserve prefab scale.");
        }
        finally
        {
            if (spawned != null)
            {
                Object.DestroyImmediate(spawned.gameObject);
            }

            Object.DestroyImmediate(spawnerObject);
            Object.DestroyImmediate(lootRoot);
            Object.DestroyImmediate(visualPrefab);
        }
    }

    private static LootItem[] LoadLootPrefabs()
    {
        List<LootItem> lootPrefabs = new List<LootItem>();
        for (int i = 0; i < LootBasePrefabPaths.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LootBasePrefabPaths[i]);
            if (prefab == null)
            {
                Debug.LogError($"Could not load loot prefab at {LootBasePrefabPaths[i]}.");
                continue;
            }

            LootItem lootItem = prefab.GetComponent<LootItem>();
            if (lootItem == null)
            {
                Debug.LogError($"Loot prefab at {LootBasePrefabPaths[i]} does not have a LootItem component.");
                continue;
            }

            lootPrefabs.Add(lootItem);
        }

        return lootPrefabs.ToArray();
    }

    private static int DeactivateSceneTemplateLoot(LootSpawner[] spawners)
    {
        int deactivatedCount = 0;
        LootItem[] sceneLootItems = Object.FindObjectsByType<LootItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < sceneLootItems.Length; i++)
        {
            if (!sceneLootItems[i].gameObject.scene.IsValid() || IsRuntimeSpawn(sceneLootItems[i]))
            {
                continue;
            }

            if (IsSceneTemplateLoot(sceneLootItems[i]))
            {
                Undo.RecordObject(sceneLootItems[i].gameObject, "Deactivate scene template loot");
                sceneLootItems[i].gameObject.SetActive(false);
                EditorUtility.SetDirty(sceneLootItems[i].gameObject);
                EditorSceneManager.MarkSceneDirty(sceneLootItems[i].gameObject.scene);
                deactivatedCount++;
            }
        }

        return deactivatedCount;
    }

    private static bool IsRuntimeSpawn(LootItem lootItem)
    {
        return lootItem.GetComponent<SpawnedLootMarker>() != null || lootItem.name.EndsWith("(Clone)");
    }

    private static bool HasEnabledChildRenderer(Transform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Renderer[] childRenderers = root.GetChild(i).GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j < childRenderers.Length; j++)
            {
                if (childRenderers[j].enabled)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasAssignedVisual(LootSpawner spawner, string prefabName)
    {
        if (spawner.lootVisualPrefabs == null)
        {
            return false;
        }

        for (int i = 0; i < spawner.lootVisualPrefabs.Length; i++)
        {
            if (spawner.lootVisualPrefabs[i] != null && spawner.lootVisualPrefabs[i].name == prefabName)
            {
                return true;
            }
        }

        return false;
    }

    private static Material GetLootMaterial(string prefabName, int slotIndex)
    {
        if (prefabName == "LootVisual_Can_Blue")
        {
            return slotIndex switch
            {
                0 => GetOrCreateMaterial("Loot_Can_Blue_Label", ColorFromHex("#2868b7"), 0f, 0.35f),
                1 => GetOrCreateMaterial("Loot_Can_Blue_Body", ColorFromHex("#3f84c9"), 0f, 0.28f),
                2 => GetOrCreateMaterial("Loot_Can_Grime", ColorFromHex("#2d3424"), 0f, 0.45f),
                3 => GetOrCreateMaterial("Loot_Dull_Metal", ColorFromHex("#8f948d"), 0.2f, 0.32f),
                _ => GetOrCreateMaterial("Loot_Dark_Interior", ColorFromHex("#151719"), 0f, 0.55f),
            };
        }

        if (prefabName == "LootVisual_Can_Red")
        {
            return slotIndex switch
            {
                0 => GetOrCreateMaterial("Loot_Can_Red_Label", ColorFromHex("#b9362d"), 0f, 0.35f),
                1 => GetOrCreateMaterial("Loot_Can_Red_Body", ColorFromHex("#c85a3d"), 0f, 0.28f),
                2 => GetOrCreateMaterial("Loot_Can_Grime", ColorFromHex("#2d3424"), 0f, 0.45f),
                3 => GetOrCreateMaterial("Loot_Dull_Metal", ColorFromHex("#8f948d"), 0.2f, 0.32f),
                _ => GetOrCreateMaterial("Loot_Dark_Interior", ColorFromHex("#151719"), 0f, 0.55f),
            };
        }

        if (prefabName == "LootVisual_Can_Green")
        {
            return slotIndex switch
            {
                0 => GetOrCreateMaterial("Loot_Can_Green_Label", ColorFromHex("#2f8f4f"), 0f, 0.35f),
                1 => GetOrCreateMaterial("Loot_Can_Green_Body", ColorFromHex("#56a968"), 0f, 0.28f),
                2 => GetOrCreateMaterial("Loot_Can_Grime", ColorFromHex("#2d3424"), 0f, 0.45f),
                3 => GetOrCreateMaterial("Loot_Dull_Metal", ColorFromHex("#8f948d"), 0.2f, 0.32f),
                _ => GetOrCreateMaterial("Loot_Dark_Interior", ColorFromHex("#151719"), 0f, 0.55f),
            };
        }

        if (prefabName == "LootVisual_Tire")
        {
            return slotIndex switch
            {
                0 => GetOrCreateMaterial("Loot_Tire_Rubber", ColorFromHex("#080807"), 0f, 0.75f),
                1 => GetOrCreateMaterial("Loot_Tire_Algae", ColorFromHex("#315226"), 0f, 0.65f),
                _ => GetOrCreateMaterial("Loot_Tire_Worn", ColorFromHex("#24221f"), 0f, 0.7f),
            };
        }

        if (prefabName == "LootVisual_Microwave")
        {
            return slotIndex switch
            {
                0 => GetOrCreateMaterial("Loot_Microwave_Grime", ColorFromHex("#3d3930"), 0f, 0.55f),
                1 => GetOrCreateMaterial("Loot_Microwave_Body", ColorFromHex("#cfc7ad"), 0f, 0.38f),
                2 => GetOrCreateMaterial("Loot_Rust", ColorFromHex("#9f4d22"), 0f, 0.65f),
                3 => GetOrCreateMaterial("Loot_Button_Dark", ColorFromHex("#1b1a1a"), 0f, 0.5f),
                4 => GetOrCreateMaterial("Loot_Dull_Metal", ColorFromHex("#8f948d"), 0.2f, 0.32f),
                5 => GetOrCreateMaterial("Loot_Dark_Glass", ColorFromHex("#121923"), 0f, 0.18f),
                _ => GetOrCreateMaterial("Loot_Dark_Rubber", ColorFromHex("#10100f"), 0f, 0.65f),
            };
        }

        if (prefabName == "LootVisual_Toaster")
        {
            return slotIndex switch
            {
                0 => GetOrCreateMaterial("Loot_Toaster_Body", ColorFromHex("#b7afa0"), 0.1f, 0.28f),
                1 => GetOrCreateMaterial("Loot_Dark_Metal", ColorFromHex("#2b3032"), 0.25f, 0.42f),
                2 => GetOrCreateMaterial("Loot_Rust", ColorFromHex("#9f4d22"), 0f, 0.65f),
                3 => GetOrCreateMaterial("Loot_Dark_Rubber", ColorFromHex("#10100f"), 0f, 0.65f),
                _ => GetOrCreateMaterial("Loot_Copper", ColorFromHex("#9b5a35"), 0.15f, 0.38f),
            };
        }

        return GetOrCreateMaterial("Loot_Dull_Metal", ColorFromHex("#8f948d"), 0.2f, 0.32f);
    }

    private static Material GetOrCreateMaterial(string materialName, Color baseColor, float metallic, float smoothness)
    {
        Directory.CreateDirectory(JunkMaterialFolder);

        string path = $"{JunkMaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.name = materialName;
        SetMaterialColor(material, baseColor);
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", metallic);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }

        if (material.HasProperty("_Color"))
        {
            return material.GetColor("_Color");
        }

        return Color.white;
    }

    private static bool IsDistinctiveColor(Color color)
    {
        float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        float min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        return max - min > 0.08f;
    }

    private static Color ColorFromHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }

        return Color.magenta;
    }

    private static GameObject FindSceneGameObject(string name)
    {
        GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].name == name && gameObjects[i].scene.IsValid())
            {
                return gameObjects[i];
            }
        }

        return null;
    }

    private static void AssertCondition(bool condition, string message)
    {
        if (!condition)
        {
            throw new System.InvalidOperationException(message);
        }
    }

    private static void AssertUniformScale(Vector3 actual, float expected, string context)
    {
        AssertVectorScale(actual, Vector3.one * expected, context);
    }

    private static void AssertVectorScale(Vector3 actual, Vector3 expected, string context)
    {
        bool matches = Mathf.Approximately(actual.x, expected.x)
            && Mathf.Approximately(actual.y, expected.y)
            && Mathf.Approximately(actual.z, expected.z);
        AssertCondition(matches, $"{context} scale was {actual}, expected {expected}.");
    }

    private static bool IsSceneTemplateLoot(LootItem lootItem)
    {
        for (int i = 0; i < LootBasePrefabPaths.Length; i++)
        {
            string prefabName = Path.GetFileNameWithoutExtension(LootBasePrefabPaths[i]);
            if (lootItem.name == prefabName)
            {
                return true;
            }
        }

        return false;
    }
}
