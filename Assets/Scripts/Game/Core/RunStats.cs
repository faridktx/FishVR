using UnityEngine;

public class RunStats : MonoBehaviour
{
    [Header("Starting Values")]
    public int startingAmmo = 5;
    public int startingCoins;

    [Header("Live Values")]
    public int ammo;
    public int coins;
    public int shieldCharges;
    public float currentHaulWeight;

    private void Awake()
    {
        ResetRun();
    }

    public void ResetRun()
    {
        ammo = startingAmmo;
        coins = startingCoins;
        shieldCharges = 0;
        currentHaulWeight = 0f;
    }

    public void ConsumeAmmo(int amount = 1)
    {
        ammo = Mathf.Max(0, ammo - amount);
    }

    public void AddAmmo(int amount)
    {
        ammo = Mathf.Max(0, ammo + amount);
    }

    public void AddCoins(int amount)
    {
        coins = Mathf.Max(0, coins + amount);
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
        {
            return false;
        }

        coins -= amount;
        return true;
    }

    public void SetHaulWeight(float value)
    {
        currentHaulWeight = Mathf.Max(0f, value);
    }
}
