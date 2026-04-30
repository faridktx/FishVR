using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.UI;

public class MvpMenuController : MonoBehaviour
{
    public GameManager gameManager;
    public RunStats runStats;

    [Header("Panels")]
    public GameObject mainMenuRoot;
    public GameObject deathMenuRoot;
    public PlayerHudDisplay hudDisplay;

    [Header("VR Placement")]
    public Transform headset;
    public float menuDistance = 1.8f;
    public float menuHeightOffset = -0.05f;
    public Vector2 menuSize = new Vector2(900f, 520f);
    public float menuWorldScale = 0.002f;
    public Vector3 hudLocalPosition = new Vector3(0f, -0.38f, 1.15f);
    public Vector2 hudSize = new Vector2(520f, 170f);
    public float hudWorldScale = 0.0016f;

    [Header("Debug Keyboard Fallback")]
    public bool enableKeyboardFallback;
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

        ConfigureVrCanvases();
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

        ConfigureVrCanvases();

        if (gameManager == null)
        {
            return;
        }

        if (gameManager.CurrentPhase != lastPhase)
        {
            lastPhase = gameManager.CurrentPhase;
            RefreshVisibility();
        }

        if (!enableKeyboardFallback || Keyboard.current == null)
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

        if (showMain || showDeath)
        {
            PlaceMenuInFrontOfHeadset();
        }
    }

    private void BuildMenuUi()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rootRect = GetComponent<RectTransform>();
        if (rootRect == null)
        {
            rootRect = gameObject.AddComponent<RectTransform>();
        }

        rootRect.sizeDelta = menuSize;
        transform.localScale = Vector3.one * menuWorldScale;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.dynamicPixelsPerUnit = 24f;

        if (GetComponent<TrackedDeviceGraphicRaycaster>() == null)
        {
            gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }

        mainMenuRoot = CreatePanel("MainMenu", "FISHVR", "Catch junk, avoid the microwave bombs, sell trash, buy ammo and shields.", "START", StartRun);
        deathMenuRoot = CreatePanel("DeathMenu", "YOU DIED", "The bomb damage took your last HP.", "RETRY", StartRun);
        deathSummaryText = CreateLabel(deathMenuRoot.transform, "DeathSummary", "", 20f, new Vector2(0f, -25f), new Vector2(620f, 36f), new Color(0.9f, 0.95f, 1f));

        Button menuButton = CreateButton(deathMenuRoot.transform, "MainMenuButton", "MAIN MENU", new Vector2(0f, -135f), ReturnToMainMenu);
        menuButton.GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 56f);
        PlaceMenuInFrontOfHeadset();
    }

    private void ConfigureVrCanvases()
    {
        if (headset == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                headset = mainCamera.transform;
            }
        }

        ConfigureMenuCanvas();
        ConfigureHudCanvas();
    }

    private void ConfigureMenuCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = menuSize;
        }

        transform.localScale = Vector3.one * menuWorldScale;

        if (GetComponent<TrackedDeviceGraphicRaycaster>() == null)
        {
            gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }
    }

    private void ConfigureHudCanvas()
    {
        if (hudDisplay == null || headset == null)
        {
            return;
        }

        Canvas canvas = hudDisplay.GetComponent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform rect = hudDisplay.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = hudSize;
        }

        hudDisplay.transform.SetParent(headset, false);
        hudDisplay.transform.localPosition = hudLocalPosition;
        hudDisplay.transform.localRotation = Quaternion.identity;
        hudDisplay.transform.localScale = Vector3.one * hudWorldScale;
    }

    private void PlaceMenuInFrontOfHeadset()
    {
        if (headset == null)
        {
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(headset.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = headset.forward;
        }

        forward.Normalize();
        transform.position = headset.position + forward * Mathf.Max(0.5f, menuDistance) + Vector3.up * menuHeightOffset;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
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
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
            DontDestroyOnLoad(eventSystemObject);
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        XRUIInputModule xrInputModule = eventSystem.GetComponent<XRUIInputModule>();
        if (xrInputModule == null)
        {
            xrInputModule = eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }

        xrInputModule.enableXRInput = true;
        xrInputModule.enableMouseInput = false;
        xrInputModule.enableTouchInput = false;
        xrInputModule.enableGamepadInput = false;
        xrInputModule.enableJoystickInput = false;
        xrInputModule.enableBuiltinActionsAsFallback = false;
    }
}
