using UnityEngine;

public class RunStats : MonoBehaviour
{
    [Header("Starting Values")]
    public int startingAmmo = 5;
    public int startingCoins;
    public int startingHp = 100;
    public int maxHp = 100;

    [Header("Live Values")]
    public int ammo;
    public int coins;
    public int hp;
    public int shieldCharges;
    public float currentHaulWeight;

    public bool IsDead => hp <= 0;

    private void Awake()
    {
        ResetRun();
    }

    public void ResetRun()
    {
        maxHp = Mathf.Max(1, maxHp);
        ammo = startingAmmo;
        coins = startingCoins;
        hp = Mathf.Clamp(startingHp, 1, maxHp);
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

    public void AddShield(int amount)
    {
        shieldCharges = Mathf.Max(0, shieldCharges + amount);
    }

    public bool TryConsumeShield(int amount = 1)
    {
        if (shieldCharges < amount)
        {
            return false;
        }

        shieldCharges -= amount;
        return true;
    }

    public void Heal(int amount)
    {
        hp = Mathf.Clamp(hp + Mathf.Max(0, amount), 0, Mathf.Max(1, maxHp));
    }

    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(0, hp - Mathf.Max(0, amount));
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
