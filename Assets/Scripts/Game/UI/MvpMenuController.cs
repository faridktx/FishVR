using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MvpMenuController : MonoBehaviour
{
    public GameManager gameManager;
    public RunStats runStats;

    [Header("Panels")]
    public GameObject mainMenuRoot;
    public GameObject deathMenuRoot;
    public PlayerHudDisplay hudDisplay;

    [Header("Keys")]
    public Key startKey = Key.Enter;
    public Key retryKey = Key.R;
    public Key menuKey = Key.M;

    private TMP_Text deathSummaryText;
    private GamePhase lastPhase;
    private GameManager subscribedGameManager;

    private void Awake()
    {
        EnsureEventSystem();

        if (mainMenuRoot == null || deathMenuRoot == null)
        {
            BuildMenuUi();
        }
    }

    private void Start()
    {
        if (mainMenuRoot == null || deathMenuRoot == null)
        {
            BuildMenuUi();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (runStats == null)
        {
            runStats = FindFirstObjectByType<RunStats>();
        }

        if (hudDisplay == null)
        {
            hudDisplay = FindFirstObjectByType<PlayerHudDisplay>();
        }

        if (hudDisplay != null && runStats != null)
        {
            hudDisplay.runStats = runStats;
            hudDisplay.bindToRunStats = true;
        }

        SubscribeToPhaseChanges();
        lastPhase = gameManager != null ? gameManager.CurrentPhase : GamePhase.MainMenu;
        RefreshVisibility();
    }

    private void OnDisable()
    {
        if (subscribedGameManager != null)
        {
            subscribedGameManager.OnPhaseChanged -= HandlePhaseChanged;
            subscribedGameManager = null;
        }
    }

    private void Update()
    {
        if (mainMenuRoot == null || deathMenuRoot == null)
        {
            BuildMenuUi();
            RefreshVisibility();
        }

        if (gameManager == null)
        {
            return;
        }

        if (gameManager.CurrentPhase != lastPhase)
        {
            lastPhase = gameManager.CurrentPhase;
            RefreshVisibility();
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (gameManager.CurrentPhase == GamePhase.MainMenu && Keyboard.current[startKey].wasPressedThisFrame)
        {
            StartRun();
        }

        if (gameManager.CurrentPhase == GamePhase.Death && Keyboard.current[retryKey].wasPressedThisFrame)
        {
            StartRun();
        }

        if (gameManager.CurrentPhase == GamePhase.Death && Keyboard.current[menuKey].wasPressedThisFrame)
        {
            ReturnToMainMenu();
        }
    }

    public void StartRun()
    {
        gameManager?.StartRun();
        RefreshVisibility();
    }

    public void ReturnToMainMenu()
    {
        gameManager?.ReturnToMainMenu();
        RefreshVisibility();
    }

    private void SubscribeToPhaseChanges()
    {
        if (gameManager == null || subscribedGameManager == gameManager)
        {
            return;
        }

        if (subscribedGameManager != null)
        {
            subscribedGameManager.OnPhaseChanged -= HandlePhaseChanged;
        }

        subscribedGameManager = gameManager;
        subscribedGameManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        lastPhase = phase;
        RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        GamePhase phase = gameManager != null ? gameManager.CurrentPhase : GamePhase.MainMenu;
        bool showMain = phase == GamePhase.MainMenu;
        bool showDeath = phase == GamePhase.Death;

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(showMain);
        }

        if (deathMenuRoot != null)
        {
            deathMenuRoot.SetActive(showDeath);
        }

        if (hudDisplay != null)
        {
            hudDisplay.gameObject.SetActive(!showMain);
        }

        if (deathSummaryText != null && runStats != null)
        {
            deathSummaryText.text = "HP " + runStats.hp + "/" + runStats.maxHp + "   Coins " + runStats.coins;
        }
    }

    private void BuildMenuUi()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        mainMenuRoot = CreatePanel("MainMenu", "FISHVR", "Catch junk, avoid the microwave bombs, sell trash, buy ammo and shields.", "START", StartRun);
        deathMenuRoot = CreatePanel("DeathMenu", "YOU DIED", "The bomb damage took your last HP.", "RETRY", StartRun);
        deathSummaryText = CreateLabel(deathMenuRoot.transform, "DeathSummary", "", 20f, new Vector2(0f, -25f), new Vector2(620f, 36f), new Color(0.9f, 0.95f, 1f));

        Button menuButton = CreateButton(deathMenuRoot.transform, "MainMenuButton", "MAIN MENU", new Vector2(0f, -135f), ReturnToMainMenu);
        menuButton.GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 56f);
    }

    private GameObject CreatePanel(string name, string title, string subtitle, string buttonText, UnityEngine.Events.UnityAction action)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(transform, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = panel.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.025f, 0.03f, 0.92f);

        CreateLabel(panel.transform, name + "Title", title, 58f, new Vector2(0f, 100f), new Vector2(720f, 80f), Color.white);
        CreateLabel(panel.transform, name + "Subtitle", subtitle, 22f, new Vector2(0f, 35f), new Vector2(760f, 70f), new Color(0.75f, 0.9f, 1f));
        CreateButton(panel.transform, name + "PrimaryButton", buttonText, new Vector2(0f, -70f), action);

        return panel;
    }

    private static TMP_Text CreateLabel(Transform parent, string name, string text, float fontSize, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        TextMeshProUGUI textMesh = go.GetComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.textWrappingMode = TextWrappingModes.Normal;

        return textMesh;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(300f, 64f);

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.1f, 0.55f, 0.75f, 1f);

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        CreateLabel(go.transform, "Label", label, 24f, Vector2.zero, rect.sizeDelta, Color.white);
        return button;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        DontDestroyOnLoad(eventSystem);
    }
}
