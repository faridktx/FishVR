using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHudDisplay : MonoBehaviour
{
    [Header("Binding")]
    public RunStats runStats;
    public bool bindToRunStats = true;

    [Header("Values")]
    public int currentAmmo = 12;
    public int maxAmmo = 12;
    public int money;
    public int currentHp = 100;
    public int maxHp = 100;
    public int currentShield = 50;
    public int maxShield = 50;

    [Header("Text")]
    public TMP_Text ammoText;
    public TMP_Text moneyText;
    public TMP_Text hpText;
    public TMP_Text shieldText;

    [Header("Bars")]
    public Image hpFill;
    public Image shieldFill;

    private void Awake()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        SyncFromRunStats();
        Refresh();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Refresh();
    }
#endif

    public void SetValues(int ammo, int ammoCapacity, int wallet, int hp, int hpCapacity, int shield, int shieldCapacity)
    {
        currentAmmo = ammo;
        maxAmmo = ammoCapacity;
        money = wallet;
        currentHp = hp;
        maxHp = hpCapacity;
        currentShield = shield;
        maxShield = shieldCapacity;
        Refresh();
    }

    private void SyncFromRunStats()
    {
        if (!bindToRunStats || runStats == null)
        {
            return;
        }

        currentAmmo = runStats.ammo;
        maxAmmo = Mathf.Max(maxAmmo, currentAmmo);
        money = runStats.coins;
        currentHp = runStats.hp;
        maxHp = Mathf.Max(1, runStats.maxHp);
        currentShield = runStats.shieldCharges;
        maxShield = Mathf.Max(1, maxShield, currentShield);
    }

    private void Refresh()
    {
        maxAmmo = Mathf.Max(0, maxAmmo);
        maxHp = Mathf.Max(1, maxHp);
        maxShield = Mathf.Max(1, maxShield);

        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        currentShield = Mathf.Clamp(currentShield, 0, maxShield);
        money = Mathf.Max(0, money);

        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{maxAmmo}";
        }

        if (moneyText != null)
        {
            moneyText.text = $"${money}";
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHp}/{maxHp}";
        }

        if (shieldText != null)
        {
            shieldText.text = $"{currentShield}/{maxShield}";
        }

        if (hpFill != null)
        {
            hpFill.fillAmount = (float)currentHp / maxHp;
        }

        if (shieldFill != null)
        {
            shieldFill.fillAmount = (float)currentShield / maxShield;
        }
    }
}
