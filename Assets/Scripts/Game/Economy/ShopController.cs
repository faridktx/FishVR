using UnityEngine;
using UnityEngine.InputSystem;

public class ShopController : MonoBehaviour
{
    public GameManager gameManager;
    public UpgradeCatalog catalog;
    public UpgradeApplier applier;

    [Header("UI")]
    public GameObject shopPanel;
    public bool enableKeyboardFallback;
    public KeyCode toggleKey = KeyCode.Tab;

    private bool isOpen;

    private void Start()
    {
        SetOpen(false);
    }

    private void Update()
    {
        if (enableKeyboardFallback && IsKeyPressed(toggleKey))
        {
            SetOpen(!isOpen);
        }
    }

    public void OpenShopPhase()
    {
        SetOpen(true);
    }

    public void CloseAndContinue()
    {
        SetOpen(false);
    }

    public void BuyAmmo()
    {
        applier?.BuyAmmo(catalog);
    }

    public void BuyMagnet()
    {
        applier?.BuyMagnet(catalog);
    }

    public void BuyReel()
    {
        applier?.BuyReel(catalog);
    }

    public void BuyShield()
    {
        applier?.BuyShield(catalog);
    }

    private void SetOpen(bool value)
    {
        isOpen = value;
        if (shopPanel != null)
        {
            shopPanel.SetActive(isOpen);
        }
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
