using UnityEngine;
using UnityEngine.InputSystem;

public class DebugHotkeys : MonoBehaviour
{
    public GameManager gameManager;
    public RunStats runStats;
    public LootSpawner lootSpawner;
    public ShopController shopController;

    public KeyCode spawnLootKey = KeyCode.F1;
    public KeyCode addAmmoKey = KeyCode.F2;
    public KeyCode addCoinsKey = KeyCode.F3;
    public KeyCode forceShopKey = KeyCode.F4;

    private void Update()
    {
        if (IsKeyPressed(spawnLootKey))
        {
            lootSpawner?.SpawnInitialLoot();
        }

        if (IsKeyPressed(addAmmoKey) && runStats != null)
        {
            runStats.AddAmmo(3);
        }

        if (IsKeyPressed(addCoinsKey) && runStats != null)
        {
            runStats.AddCoins(10);
        }

        if (IsKeyPressed(forceShopKey) && shopController != null)
        {
            shopController.OpenShopPhase();
        }
    }

    private static bool IsKeyPressed(KeyCode keyCode)
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        Key key = (Key)keyCode;
        return Keyboard.current[key].wasPressedThisFrame;
    }
}
