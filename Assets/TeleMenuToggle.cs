using UnityEngine;
using UnityEngine.InputSystem;

public class TeleMenuToggle : MonoBehaviour
{
    public InputActionReference menuButton;
    public GameObject menuCanvas;

    private void OnEnable()
    {
        if (menuButton != null && menuButton.action != null)
            menuButton.action.Enable();
    }

    private void OnDisable()
    {
        if (menuButton != null && menuButton.action != null)
            menuButton.action.Disable();
    }

    private void Update()
    {
        if (menuButton == null || menuButton.action == null || menuCanvas == null) return;

        if (menuButton.action.WasPressedThisFrame())
        {
            menuCanvas.SetActive(!menuCanvas.activeSelf);
            Debug.Log("Button Pressed!");
        }
    }
}