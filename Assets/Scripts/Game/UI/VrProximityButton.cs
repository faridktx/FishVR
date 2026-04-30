using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class VrProximityButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private float pressCooldown = 0.5f;
    [SerializeField] private float colliderDepth = 24f;

    private bool hasPressed;
    private float nextPressTime;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryPress(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryPress(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsInteractorCollider(other))
        {
            hasPressed = false;
        }
    }

    private void TryPress(Collider other)
    {
        if (button == null || !button.interactable || !IsInteractorCollider(other))
        {
            return;
        }

        if (hasPressed || Time.time < nextPressTime)
        {
            return;
        }

        hasPressed = true;
        nextPressTime = Time.time + pressCooldown;

        if (EventSystem.current != null)
        {
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        }
        else
        {
            button.onClick.Invoke();
        }
    }

    private void EnsureTriggerCollider()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        BoxCollider trigger = GetComponent<BoxCollider>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<BoxCollider>();
        }

        trigger.isTrigger = true;
        trigger.size = new Vector3(Mathf.Max(1f, rectTransform.rect.width), Mathf.Max(1f, rectTransform.rect.height), colliderDepth);
        trigger.center = Vector3.zero;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.isKinematic = true;
        body.useGravity = false;
    }

    public static bool IsInteractorCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        Transform current = other.transform;
        while (current != null)
        {
            string name = current.name;
            if (name.Contains("Poke Interactor") ||
                name.Contains("Direct Interactor") ||
                name.Contains("Hand") ||
                name.Contains("Controller"))
            {
                return true;
            }

            if (current.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor>() != null ||
                current.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>() != null)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}
