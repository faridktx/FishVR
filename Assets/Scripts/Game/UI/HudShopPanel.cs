using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Self-contained shop panel for the HUD prototype scene.
/// Works with RunStats to buy ammo and shields using coins.
/// Toggle with Tab key; buy with keyboard or on-screen buttons.
/// </summary>
public class HudShopPanel : MonoBehaviour
{
    [Header("Economy References")]
    public RunStats runStats;
    public GameManager gameManager;

    [Header("Prices & Amounts")]
    public int ammoCost = 5;
    public int ammoAmount = 2;
    public int shieldCost = 12;
    public int shieldAmount = 1;

    [Header("HUD Binding")]
    public PlayerHudDisplay hudDisplay;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip shopBuyClip;

    [Header("UI References (auto-built if null)")]
    public GameObject panelRoot;
    public TMP_Text ammoBtnLabel;
    public TMP_Text shieldBtnLabel;
    public TMP_Text feedbackLabel;
    public Button ammoBuyButton;
    public Button shieldBuyButton;

    [Header("Toggle")]
    public bool enableKeyboardFallback;
    public Key toggleKey = Key.Tab;

    private bool isOpen;
    private float feedbackTimer;
    private const float FeedbackDuration = 1.5f;

    private void Awake()
    {
        if (panelRoot == null)
        {
            BuildUI();
        }

        SetOpen(false);
    }

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        SyncHud();
        
        // Wire up physical VR shop cards if they exist
        GameObject ammoCard = GameObject.Find("Card_Ammo");
        if (ammoCard != null)
        {
            var visual = ammoCard.GetComponent<ShopCardVisual>();
            if (visual != null)
            {
                visual.onCardPressed.AddListener(BuyAmmo);
            }
        }
        
        GameObject shieldCard = GameObject.Find("Card_Shield");
        if (shieldCard != null)
        {
            var visual = shieldCard.GetComponent<ShopCardVisual>();
            if (visual != null)
            {
                visual.onCardPressed.AddListener(BuyShield);
            }
        }
    }

    private void Update()
    {
        if (!enableKeyboardFallback || Keyboard.current == null)
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0f && feedbackLabel != null)
                {
                    feedbackLabel.text = "";
                }
            }

            SyncHud();
            return;
        }

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            SetOpen(!isOpen);
        }

        if (isOpen && Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            BuyAmmo();
        }

        if (isOpen && Keyboard.current[Key.Digit2].wasPressedThisFrame)
        {
            BuyShield();
        }

        if (isOpen)
        {
            RefreshLabels();
        }

        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0f && feedbackLabel != null)
            {
                feedbackLabel.text = "";
            }
        }

        SyncHud();
    }

    public void BuyAmmo()
    {
        if (runStats == null)
        {
            ShowFeedback("Not enough coins!");
            return;
        }

        if (runStats.coins < ammoCost)
        {
            ShowFeedback("Not enough coins!");
            return;
        }

        runStats.SpendCoins(ammoCost);
        runStats.AddAmmo(ammoAmount);
        ResumeRoundIfOutOfAmmoStateRecovered();
        PlayUiOneShot(shopBuyClip);
        ShowFeedback("Bought +" + ammoAmount + " Ammo!");
        SyncHud();
    }

    public void BuyShield()
    {
        if (runStats == null)
        {
            ShowFeedback("Not enough coins!");
            return;
        }

        if (runStats.coins < shieldCost)
        {
            ShowFeedback("Not enough coins!");
            return;
        }

        runStats.SpendCoins(shieldCost);
        runStats.AddShield(Mathf.Max(1, shieldAmount));
        PlayUiOneShot(shopBuyClip);
        ShowFeedback("Bought +" + shieldAmount + " Shield!");
        SyncHud();
    }

    private void SyncHud()
    {
        if (hudDisplay == null || runStats == null)
        {
            return;
        }

        hudDisplay.currentAmmo = runStats.ammo;
        hudDisplay.maxAmmo = Mathf.Max(hudDisplay.maxAmmo, runStats.ammo);
        hudDisplay.money = runStats.coins;
        hudDisplay.currentHp = runStats.hp;
        hudDisplay.maxHp = runStats.maxHp;
        hudDisplay.currentShield = runStats.shieldCharges;
        hudDisplay.maxShield = Mathf.Max(1, hudDisplay.maxShield, runStats.shieldCharges);
    }

    private void ShowFeedback(string msg)
    {
        if (feedbackLabel != null)
        {
            feedbackLabel.text = msg;
        }

        feedbackTimer = FeedbackDuration;
    }

    private void ResumeRoundIfOutOfAmmoStateRecovered()
    {
        if (gameManager == null || runStats == null)
        {
            return;
        }

        if (runStats.ammo > 0 && gameManager.CurrentPhase == GamePhase.RoundOver)
        {
            gameManager.SetPhase(GamePhase.AimShoot);
        }
    }

    private void RefreshLabels()
    {
        if (ammoBtnLabel != null)
        {
            ammoBtnLabel.text = "[1] Buy Ammo  (" + ammoCost + " coins)";
        }

        if (shieldBtnLabel != null)
        {
            shieldBtnLabel.text = "[2] Buy Shield  (" + shieldCost + " coins)";
        }
    }

    public void SetOpen(bool value)
    {
        if (isOpen == value)
        {
            return;
        }

        isOpen = value;
        if (panelRoot != null)
        {
            panelRoot.SetActive(isOpen);
        }

    }

    private void PlayUiOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource source = uiAudioSource != null ? uiAudioSource : GetComponent<AudioSource>();
        if (source != null)
        {
            source.PlayOneShot(clip);
        }
    }

    // ── Auto-build a minimal UI so the prototype scene works without manual setup ──
    private void BuildUI()
    {
        // Panel root (dark semi-transparent background)
        panelRoot = new GameObject("ShopPanelRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelRoot.transform.SetParent(transform, false);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(400f, 220f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelBg = panelRoot.GetComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);

        // Title
        CreateLabel(panelRoot.transform, "ShopTitle", "SHOP  (Tab to close)", 24,
            new Vector2(0f, 80f), new Vector2(380f, 40f), new Color(1f, 0.85f, 0.3f));

        // Ammo button row
        ammoBtnLabel = CreateLabel(panelRoot.transform, "AmmoBtnLabel", "[1] Buy Ammo", 18,
            new Vector2(0f, 20f), new Vector2(360f, 36f), Color.white);

        // Shield button row
        shieldBtnLabel = CreateLabel(panelRoot.transform, "ShieldBtnLabel", "[2] Buy Shield", 18,
            new Vector2(0f, -25f), new Vector2(360f, 36f), Color.white);

        // Feedback
        feedbackLabel = CreateLabel(panelRoot.transform, "FeedbackLabel", "", 16,
            new Vector2(0f, -80f), new Vector2(360f, 30f), new Color(0.3f, 1f, 0.5f));
    }

    private static TMP_Text CreateLabel(Transform parent, string name, string text, float fontSize,
        Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;

        return tmp;
    }
}
