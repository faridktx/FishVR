using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ReturnToWaistOnRelease : MonoBehaviour
{
    [Header("Camera Holster")]
    public Transform headsetTarget;
    public float returnDelay = 0.5f;
    public bool matchTargetRotation = true;
    public bool followYawOnly = true;
    public bool parentToTargetWhileHolstered = true;
    public bool captureOffsetOnStart = true;
    public Vector3 localPositionOffset;
    public Vector3 localEulerOffset;

    [Header("Desktop Override")]
    public bool enableDesktopOverride = false;
    public float desktopForwardDistance = 0.9f;
    public float desktopVerticalOffset = -0.2f;
    public bool desktopUseCameraPitch = false;
    public Vector3 desktopAdditionalOffset;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody body;
    private float returnAtTime = -1f;
    private bool isHeld;
    private bool isHolstered;
    private bool initialUseGravity;
    private bool initialIsKinematic;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        body = GetComponent<Rigidbody>();

        if (headsetTarget == null && Camera.main != null)
        {
            headsetTarget = Camera.main.transform;
        }

        if (body != null)
        {
            initialUseGravity = body.useGravity;
            initialIsKinematic = body.isKinematic;
        }
    }

    private void Start()
    {
        Transform target = headsetTarget;
        if (target != null)
        {
            if (captureOffsetOnStart)
            {
                Quaternion baseRotation = followYawOnly ? GetTargetYawRotation(target) : target.rotation;
                localPositionOffset = Quaternion.Inverse(baseRotation) * (transform.position - target.position);
                Quaternion relativeRotation = Quaternion.Inverse(baseRotation) * transform.rotation;
                localEulerOffset = relativeRotation.eulerAngles;
            }

            ApplyHolsterPose(target);
            isHolstered = true;
        }
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void Update()
    {
        Transform target = headsetTarget;

        if (isHeld || target == null)
        {
            return;
        }

        if (!isHolstered)
        {
            if (returnAtTime >= 0f && Time.time >= returnAtTime)
            {
                isHolstered = true;
                returnAtTime = -1f;
                ApplyHolsterPose(target);
            }

            return;
        }

        ApplyHolsterPose(target);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
        returnAtTime = -1f;
        isHolstered = false;

        if (parentToTargetWhileHolstered)
        {
            transform.SetParent(null, true);
        }

        if (body != null)
        {
            body.isKinematic = initialIsKinematic;
            body.useGravity = initialUseGravity;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;
        isHolstered = false;
        returnAtTime = Time.time + returnDelay;
    }

    private void ApplyHolsterPose(Transform target)
    {
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.useGravity = false;
        }

        if (enableDesktopOverride)
        {
            ApplyDesktopHolsterPose(target);
            return;
        }

        Quaternion baseRotation = followYawOnly ? GetTargetYawRotation(target) : target.rotation;
        Vector3 targetWorldPosition = target.position + baseRotation * localPositionOffset;
        Quaternion targetWorldRotation = baseRotation * Quaternion.Euler(localEulerOffset);

        if (parentToTargetWhileHolstered && !followYawOnly)
        {
            if (transform.parent != target)
            {
                transform.SetParent(target, false);
            }

            transform.localPosition = localPositionOffset;

            if (matchTargetRotation)
            {
                transform.localRotation = Quaternion.Euler(localEulerOffset);
            }

            return;
        }

        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        transform.position = targetWorldPosition;

        if (matchTargetRotation)
        {
            transform.rotation = targetWorldRotation;
        }
    }

    private void ApplyDesktopHolsterPose(Transform target)
    {
        Vector3 forward = desktopUseCameraPitch ? target.forward : Vector3.ProjectOnPlane(target.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();
        Vector3 basePosition =
            target.position +
            forward * Mathf.Max(0f, desktopForwardDistance) +
            Vector3.up * desktopVerticalOffset;

        Quaternion baseRotation = desktopUseCameraPitch ? target.rotation : GetTargetYawRotation(target);
        Vector3 targetWorldPosition = basePosition + baseRotation * desktopAdditionalOffset;

        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        transform.position = targetWorldPosition;

        if (matchTargetRotation)
        {
            transform.rotation = baseRotation * Quaternion.Euler(localEulerOffset);
        }
    }

    private static Quaternion GetTargetYawRotation(Transform target)
    {
        Vector3 euler = target.eulerAngles;
        return Quaternion.Euler(0f, euler.y, 0f);
    }
}