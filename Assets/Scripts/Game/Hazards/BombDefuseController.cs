using UnityEngine;

public class BombDefuseController : MonoBehaviour
{
    public GameManager gameManager;
    public RunStats runStats;

    public float defuseWindowSeconds = 3f;
    public int bombDamage = 50;

    [Header("Bomb Pulse")]
    public bool pulseBombWhileActive = true;
    public float pulseSpeed = 7f;
    public float pulseScaleAmount = 0.18f;

    [Header("Audio")]
    public AudioSource bombAudioSource;
    public AudioClip warningBeepClip;
    public AudioClip explosionClip;
    public float warningBeepInterval = 0.8f;
    public float finalWarningBeepInterval = 0.3f;
    public float finalWarningThreshold = 1f;

    public float RemainingTime { get; private set; }
    public LootItem ActiveBomb => activeBomb;
    public bool IsTimerActive => timerActive;

    private LootItem activeBomb;
    private bool timerActive;
    private Vector3 activeBombBaseScale;
    private bool hasBombBaseScale;
    private float nextWarningBeepTime;

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        if (gameManager.CurrentPhase != GamePhase.Defuse)
        {
            ResetActiveBombVisual();
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

        if (activeBomb == null)
        {
            ResolveSuccess();
            return;
        }

        if (!activeBomb.isDocked)
        {
            ResolveThrownAway();
            return;
        }

        ApplyPulse();
        TickWarningBeep();

        RemainingTime -= Time.deltaTime;

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
            if (item != null && item.kind == LootKind.Bomb && item.isDocked)
            {
                activeBomb = item;
                timerActive = true;
                RemainingTime = defuseWindowSeconds;
                activeBombBaseScale = activeBomb.transform.localScale;
                hasBombBaseScale = true;
                nextWarningBeepTime = 0f;
                return;
            }
        }

        gameManager.OnDefuseResolved(true);
    }

    private void ResolveSuccess()
    {
        ResetActiveBombVisual();
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
        ResetActiveBombVisual();
        timerActive = false;
        RemainingTime = 0f;

        PlayBombOneShot(explosionClip);

        if (runStats != null && runStats.TryConsumeShield())
        {
            if (activeBomb != null)
            {
                gameManager.RemoveLandedItem(activeBomb);
                Destroy(activeBomb.gameObject);
            }

            activeBomb = null;
            gameManager.OnDefuseResolved(true);
            return;
        }

        if (runStats != null)
        {
            runStats.TakeDamage(bombDamage);
        }

        activeBomb = null;
        gameManager.OnDefuseResolved(false);
    }

    private void ResolveThrownAway()
    {
        ResetActiveBombVisual();
        timerActive = false;
        RemainingTime = 0f;

        if (activeBomb != null)
        {
            gameManager.RemoveLandedItem(activeBomb);
        }

        activeBomb = null;
        gameManager.OnDefuseResolved(true);
    }

    private void ApplyPulse()
    {
        if (!pulseBombWhileActive || activeBomb == null)
        {
            return;
        }

        if (!hasBombBaseScale)
        {
            activeBombBaseScale = activeBomb.transform.localScale;
            hasBombBaseScale = true;
        }

        float pulse = 1f + Mathf.Sin(Time.time * Mathf.Max(0.1f, pulseSpeed)) * Mathf.Max(0f, pulseScaleAmount);
        activeBomb.transform.localScale = activeBombBaseScale * pulse;
    }

    private void ResetActiveBombVisual()
    {
        if (activeBomb != null && hasBombBaseScale)
        {
            activeBomb.transform.localScale = activeBombBaseScale;
        }

        hasBombBaseScale = false;
    }

    private void TickWarningBeep()
    {
        if (warningBeepClip == null)
        {
            return;
        }

        float interval = RemainingTime <= finalWarningThreshold
            ? finalWarningBeepInterval
            : warningBeepInterval;

        if (Time.time < nextWarningBeepTime)
        {
            return;
        }

        PlayBombOneShot(warningBeepClip);
        nextWarningBeepTime = Time.time + Mathf.Max(0.05f, interval);
    }

    private void PlayBombOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource source = bombAudioSource != null ? bombAudioSource : GetComponent<AudioSource>();
        if (source != null)
        {
            source.PlayOneShot(clip);
        }
    }

}
