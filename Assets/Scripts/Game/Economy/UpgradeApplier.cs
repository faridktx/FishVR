using UnityEngine;

public class UpgradeApplier : MonoBehaviour
{
    public RunStats runStats;
    public HarpoonShooter shooter;

    public void BuyAmmo(UpgradeCatalog catalog)
    {
        if (!CanUse(catalog) || !runStats.SpendCoins(catalog.ammoCost))
        {
            return;
        }

        runStats.AddAmmo(catalog.ammoAmount);
    }

    public void BuyMagnet(UpgradeCatalog catalog)
    {
        if (!CanUse(catalog) || !runStats.SpendCoins(catalog.magnetCost))
        {
            return;
        }

        if (shooter != null && shooter.projectilePrefab != null && shooter.projectilePrefab.magnetCollector != null)
        {
            shooter.projectilePrefab.magnetCollector.magnetRadius += catalog.magnetRadiusAmount;
        }
    }

    public void BuyReel(UpgradeCatalog catalog)
    {
        if (!CanUse(catalog) || !runStats.SpendCoins(catalog.reelCost))
        {
            return;
        }

        if (shooter != null && shooter.projectilePrefab != null && shooter.projectilePrefab.returnController != null)
        {
            shooter.projectilePrefab.returnController.baseReturnSpeed += catalog.reelSpeedAmount;
        }
    }

    public void BuyShield(UpgradeCatalog catalog)
    {
        if (!CanUse(catalog) || !runStats.SpendCoins(catalog.shieldCost))
        {
            return;
        }

        runStats.AddShield(Mathf.Max(1, catalog.shieldAmount));
    }

    private bool CanUse(UpgradeCatalog catalog)
    {
        return catalog != null && runStats != null;
    }
}
