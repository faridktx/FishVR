using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleUIRay : MonoBehaviour
{
    [SerializeField] private GameObject uiRayObject;
    [SerializeField] private InputActionReference showRayAction;

    private void OnEnable()
    {
        if (showRayAction != null)
            showRayAction.action.Enable();
    }

    private void OnDisable()
    {
        if (showRayAction != null)
            showRayAction.action.Disable();
    }

    private void Update()
    {
        if (uiRayObject == null || showRayAction == null)
            return;

        bool isPressed = showRayAction.action.IsPressed();
        uiRayObject.SetActive(isPressed);
    }
}