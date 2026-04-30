using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    public GameManager gameManager;
    public RunStats runStats;
    public BombDefuseController defuseController;

    private string hudContent;

    private void Update()
    {
        if (gameManager == null || runStats == null)
        {
            return;
        }

        float reelSpeed = 0f;
        if (gameManager.activeProjectile != null && gameManager.activeProjectile.ReturnController != null)
        {
            reelSpeed = gameManager.activeProjectile.ReturnController.GetCurrentReturnSpeed();
        }

        float bombTimer = defuseController != null ? defuseController.RemainingTime : 0f;

        hudContent =
            "Phase: " + gameManager.CurrentPhase + "\n" +
            "Ammo: " + runStats.ammo + "\n" +
            "Coins: " + runStats.coins + "\n" +
            "HP: " + runStats.hp + "/" + runStats.maxHp + "\n" +
            "Shield: " + runStats.shieldCharges + "\n" +
            "Haul Weight: " + runStats.currentHaulWeight.ToString("0.00") + "\n" +
            "Reel Speed: " + reelSpeed.ToString("0.00") + "\n" +
            "Bomb Timer: " + bombTimer.ToString("0.00") + "\n" +
            "Controls: LMB Shoot | E Defuse | Tab Shop";
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(hudContent))
        {
            return;
        }

        GUI.Label(new Rect(12f, 12f, 520f, 220f), hudContent);
    }
}
