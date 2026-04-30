using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(LootItem))]
public class VrLootGrabSetup : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private bool allowThrowOnRelease = true;

    private LootItem lootItem;
    private Rigidbody body;
    private Collider itemCollider;

    private void Awake()
    {
        lootItem = GetComponent<LootItem>();
        body = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();

        EnsurePhysics();
        EnsureGrabInteractable();
    }

    private void OnEnable()
    {
        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void EnsurePhysics()
    {
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (itemCollider == null)
        {
            itemCollider = gameObject.AddComponent<BoxCollider>();
        }

        itemCollider.isTrigger = false;
    }

    private void EnsureGrabInteractable()
    {
        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.smoothPosition = true;
        grabInteractable.smoothRotation = true;
        grabInteractable.throwOnDetach = allowThrowOnRelease;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        lootItem.PrepareForVrGrab();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        lootItem.HandleVrRelease();
    }
}
