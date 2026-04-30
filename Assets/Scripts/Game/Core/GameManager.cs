using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public RunStats runStats;

    [Header("Flow Anchors")]
    public Transform dockPoint;
    [FormerlySerializedAs("sortingDropPoint")]
    public Transform tableSpawnPoint;

    [Header("Landing Polish")]
    public float dockArrivalDistance = 1.0f;
    public float maxReelSeconds = 8f;
    public float dropRadius = 0.45f;
    public float settleDownwardImpulse = 0.8f;
    public float settleRandomTorque = 0.15f;
    public bool useGridDropLayout = true;
    public int gridColumns = 4;
    public float gridSpacingX = 0.45f;
    public float gridSpacingZ = 0.45f;
    public float gridVerticalStep = 0.08f;
    public float tableSettleHoldDuration = 0.12f;
    public float tableReleaseStagger = 0.015f;
    public float tableReleaseDownwardSpeed = 0.4f;

    [Header("Phase")]
    public bool startAtMainMenu = true;
    [SerializeField] private GamePhase currentPhase = GamePhase.AimShoot;

    [Header("Runtime")]
    public NetProjectile activeProjectile;
    private bool isResolvingDockLanding;
    private float reelTimer;

    private readonly List<LootItem> landedItems = new List<LootItem>();

    public GamePhase CurrentPhase => currentPhase;
    public IReadOnlyList<LootItem> LandedItems => landedItems;

    public System.Action<GamePhase> OnPhaseChanged;

    private void Start()
    {
        if (startAtMainMenu)
        {
            runStats?.ResetRun();
            SetPhase(GamePhase.MainMenu);
            return;
        }

        runStats?.ResetRun();
        SetPhase(GamePhase.AimShoot);
    }

    public void SetPhase(GamePhase phase)
    {
        currentPhase = phase;
        OnPhaseChanged?.Invoke(currentPhase);
    }

    public void OnShotFired(NetProjectile projectile)
    {
        if (activeProjectile != null && activeProjectile != projectile)
        {
            Destroy(activeProjectile.gameObject);
        }

        activeProjectile = projectile;
        isResolvingDockLanding = false;
        reelTimer = 0f;
        SetPhase(GamePhase.AutoReel);
    }

    public void OnProjectileRequestReturn(NetProjectile projectile)
    {
        if (projectile == null || dockPoint == null)
        {
            return;
        }

        projectile.StartReturnToDock(dockPoint);
    }

    public void ForceDockLanding(NetProjectile projectile)
    {
        if (projectile == null || projectile != activeProjectile)
        {
            return;
        }

        ResolveDockLanding();
    }

    private void Update()
    {
        if (currentPhase == GamePhase.AutoReel)
        {
            TickAutoReel();
        }
    }

    private void TickAutoReel()
    {
        if (activeProjectile == null)
        {
            reelTimer = 0f;
            runStats?.SetHaulWeight(0f);

            if (runStats == null || runStats.ammo <= 0)
            {
                SetPhase(GamePhase.RoundOver);
                return;
            }

            SetPhase(GamePhase.AimShoot);
            return;
        }

        if (dockPoint == null)
        {
            return;
        }

        reelTimer += Time.deltaTime;

        if (runStats != null && activeProjectile.MagnetCollector != null)
        {
            runStats.SetHaulWeight(activeProjectile.MagnetCollector.GetTotalWeight());
        }

        bool isReturning =
            activeProjectile.ReturnController != null &&
            activeProjectile.ReturnController.IsReturning;

        float sqrDistance = (activeProjectile.transform.position - dockPoint.position).sqrMagnitude;
        float arrivalSqr = dockArrivalDistance * dockArrivalDistance;
        bool arrivedAtDock = isReturning && sqrDistance <= arrivalSqr;
        bool timedOut = reelTimer >= maxReelSeconds;

        if (!arrivedAtDock && !timedOut)
        {
            return;
        }

        ResolveDockLanding();
    }

    private void ResolveDockLanding()
    {
        if (activeProjectile == null || isResolvingDockLanding)
        {
            return;
        }

        isResolvingDockLanding = true;

        if (activeProjectile.ReturnController != null)
        {
            activeProjectile.ReturnController.StopReturn();
        }

        landedItems.Clear();

        if (activeProjectile.MagnetCollector != null)
        {
            List<LootItem> detached = activeProjectile.MagnetCollector.DetachAll();
            for (int i = 0; i < detached.Count; i++)
            {
                LootItem item = detached[i];
                if (item == null)
                {
                    continue;
                }

                if (tableSpawnPoint != null)
                {
                    item.transform.position = GetDropPosition(i);
                }

                Rigidbody itemRb = item.GetComponent<Rigidbody>();
                if (itemRb != null)
                {
                    itemRb.linearVelocity = Vector3.zero;
                    itemRb.angularVelocity = Vector3.zero;
                }

                float hold = tableSettleHoldDuration + i * tableReleaseStagger;
                item.PlaceOnTable(
                    GetDropPosition(i),
                    hold,
                    tableReleaseDownwardSpeed
                );

                landedItems.Add(item);
            }
        }

        bool hasBomb = false;
        for (int i = 0; i < landedItems.Count; i++)
        {
            if (landedItems[i] != null && landedItems[i].kind == LootKind.Bomb)
            {
                hasBomb = true;
                break;
            }
        }

        Destroy(activeProjectile.gameObject);
        activeProjectile = null;
        reelTimer = 0f;
        runStats?.SetHaulWeight(0f);

        if (hasBomb)
        {
            SetPhase(GamePhase.Defuse);
            isResolvingDockLanding = false;
            return;
        }

        if (runStats == null || runStats.ammo <= 0)
        {
            SetPhase(GamePhase.RoundOver);
            isResolvingDockLanding = false;
            return;
        }

        SetPhase(GamePhase.AimShoot);
        isResolvingDockLanding = false;
    }

    private Vector3 GetDropPosition(int index)
    {
        if (tableSpawnPoint == null)
        {
            return Vector3.zero;
        }

        if (!useGridDropLayout)
        {
            return tableSpawnPoint.position + Random.insideUnitSphere * dropRadius;
        }

        int cols = Mathf.Max(1, gridColumns);
        int row = index / cols;
        int col = index % cols;

        float offsetX = (col - (cols - 1) * 0.5f) * gridSpacingX;
        float offsetZ = row * gridSpacingZ;
        float offsetY = row * gridVerticalStep;

        Vector3 localOffset = new Vector3(offsetX, offsetY, offsetZ);
        return tableSpawnPoint.TransformPoint(localOffset);
    }

    public void OnDefuseResolved(bool success)
    {
        if (!success)
        {
            ClearLandedItems();
        }

        if (runStats != null && runStats.IsDead)
        {
            SetPhase(GamePhase.Death);
            return;
        }

        if (runStats == null || runStats.ammo <= 0)
        {
            SetPhase(GamePhase.RoundOver);
            return;
        }

        SetPhase(GamePhase.AimShoot);
    }

    public void OnSortingFinished()
    {
        if (runStats == null || runStats.ammo <= 0)
        {
            SetPhase(GamePhase.RoundOver);
            return;
        }

        SetPhase(GamePhase.AimShoot);
    }

    public void CloseShopAndContinueRound()
    {
        if (runStats == null || runStats.ammo <= 0)
        {
            SetPhase(GamePhase.RoundOver);
            return;
        }

        SetPhase(GamePhase.AimShoot);
    }

    public void ClearLandedItems()
    {
        for (int i = landedItems.Count - 1; i >= 0; i--)
        {
            if (landedItems[i] != null)
            {
                Destroy(landedItems[i].gameObject);
            }
        }

        landedItems.Clear();
    }

    public void RemoveLandedItem(LootItem item)
    {
        landedItems.Remove(item);
    }

    public void StartRun()
    {
        runStats?.ResetRun();
        ClearLandedItems();

        if (activeProjectile != null)
        {
            Destroy(activeProjectile.gameObject);
            activeProjectile = null;
        }

        reelTimer = 0f;
        isResolvingDockLanding = false;
        SetPhase(GamePhase.AimShoot);
    }

    public void ReturnToMainMenu()
    {
        ClearLandedItems();

        if (activeProjectile != null)
        {
            Destroy(activeProjectile.gameObject);
            activeProjectile = null;
        }

        SetPhase(GamePhase.MainMenu);
    }
}
