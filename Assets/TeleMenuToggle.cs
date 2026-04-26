using UnityEngine;
using UnityEngine.InputSystem;

public class TeleMenuToggle : MonoBehaviour
{
    public InputActionReference menuButton;
    public GameObject menuCanvas;
    public Collider[] hitboxColliders;

    private void Start()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(false);
        foreach (Collider col in hitboxColliders)
            if (col != null) col.enabled = false;
    }

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
            bool newState = !menuCanvas.activeSelf;
            menuCanvas.SetActive(newState);
            foreach (Collider col in hitboxColliders)
                if (col != null) col.enabled = newState;
        }
    }
}