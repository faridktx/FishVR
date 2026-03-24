using UnityEngine;
using UnityEngine.InputSystem;

public class BombDefuseController : MonoBehaviour
{
    public GameManager gameManager;
    public RunStats runStats;

    public float defuseWindowSeconds = 3f;
    public KeyCode defuseKey = KeyCode.E;

    public float RemainingTime { get; private set; }

    private LootItem activeBomb;
    private bool timerActive;

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        if (gameManager.CurrentPhase != GamePhase.Defuse)
        {
            timerActive = false;
            RemainingTime = 0f;
            activeBomb = null;
            return;
        }

        if (!timerActive)
        {
            BeginDefuse();
        }

        if (!timerActive)
        {
            return;
        }

        RemainingTime -= Time.deltaTime;

        if (IsKeyPressed(defuseKey))
        {
            ResolveSuccess();
            return;
        }

        if (RemainingTime <= 0f)
        {
            ResolveFailure();
        }
    }

    private void BeginDefuse()
    {
        for (int i = 0; i < gameManager.LandedItems.Count; i++)
        {
            LootItem item = gameManager.LandedItems[i];
            if (item != null && item.kind == LootKind.Bomb)
            {
                activeBomb = item;
                timerActive = true;
                RemainingTime = defuseWindowSeconds;
                return;
            }
        }

        gameManager.OnDefuseResolved(true);
    }

    private void ResolveSuccess()
    {
        timerActive = false;
        RemainingTime = 0f;

        if (activeBomb != null)
        {
            gameManager.RemoveLandedItem(activeBomb);
            Destroy(activeBomb.gameObject);
        }

        activeBomb = null;
        gameManager.OnDefuseResolved(true);
    }

    private void ResolveFailure()
    {
        timerActive = false;
        RemainingTime = 0f;

        if (runStats != null && runStats.shieldCharges > 0)
        {
            runStats.shieldCharges -= 1;
            if (activeBomb != null)
            {
                gameManager.RemoveLandedItem(activeBomb);
                Destroy(activeBomb.gameObject);
            }

            activeBomb = null;
            gameManager.OnDefuseResolved(true);
            return;
        }

        activeBomb = null;
        gameManager.OnDefuseResolved(false);
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
