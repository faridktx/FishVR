using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class SortableItem : MonoBehaviour
{
    public KeyCode dropKey = KeyCode.Mouse1;

    private static SortableItem currentlyHeld;
    private static Camera cachedCamera;

    private Rigidbody rb;

    public bool IsHeld => currentlyHeld == this;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnMouseDown()
    {
        if (currentlyHeld != null && currentlyHeld != this)
        {
            currentlyHeld.Release();
        }

        Hold();
    }

    private void Update()
    {
        if (!IsHeld)
        {
            return;
        }

        Camera cam = GetCamera();
        if (cam == null)
        {
            return;
        }

        Vector3 target = cam.transform.position + cam.transform.forward * 1.3f;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 18f);

        if (IsKeyPressed(dropKey))
        {
            Release();
        }
    }

    private void Hold()
    {
        currentlyHeld = this;
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void Release()
    {
        if (currentlyHeld == this)
        {
            currentlyHeld = null;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    public static SortableItem GetHeldItem()
    {
        return currentlyHeld;
    }

    private Camera GetCamera()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        return cachedCamera;
    }

    private static bool IsKeyPressed(KeyCode keyCode)
    {
        if (keyCode == KeyCode.Mouse0)
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        if (keyCode == KeyCode.Mouse1)
        {
            return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        }

        if (keyCode == KeyCode.Mouse2)
        {
            return Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame;
        }

        if (Keyboard.current == null)
        {
            return false;
        }

        Key key = (Key)keyCode;
        return Keyboard.current[key].wasPressedThisFrame;
    }
}
