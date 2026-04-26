using UnityEngine;

[DisallowMultipleComponent]
public class XRMenuTeleportAction : MonoBehaviour
{
    private enum DestinationReferenceMode
    {
        Feet,
        Head
    }

    [Header("Player References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform headTransform;

    [Header("Teleport Target")]
    [SerializeField] private Transform destination;
    [SerializeField] private bool alignHeadYawToDestination = true;
    [SerializeField] private DestinationReferenceMode destinationReferenceMode = DestinationReferenceMode.Feet;

    private void Reset()
    {
        if (headTransform == null && Camera.main != null)
            headTransform = Camera.main.transform;

        if (playerRoot == null && headTransform != null && headTransform.parent != null)
            playerRoot = headTransform.parent;
    }

    public void TeleportPlayer()
    {
        if (playerRoot == null || headTransform == null || destination == null)
        {
            Debug.LogWarning("XRMenuTeleportAction is missing required references.", this);
            return;
        }

        CharacterController characterController = playerRoot.GetComponent<CharacterController>();
        Rigidbody rootRigidbody = playerRoot.GetComponent<Rigidbody>();

        if (characterController != null)
            characterController.enabled = false;

        if (alignHeadYawToDestination)
        {
            float yawDelta = Mathf.DeltaAngle(headTransform.eulerAngles.y, destination.eulerAngles.y);
            playerRoot.RotateAround(headTransform.position, Vector3.up, yawDelta);
        }

        Vector3 currentHeadOffset = headTransform.position - playerRoot.position;
        Vector3 targetRootPosition = destination.position - new Vector3(currentHeadOffset.x, 0f, currentHeadOffset.z);

        if (destinationReferenceMode == DestinationReferenceMode.Feet)
        {
            targetRootPosition.y = destination.position.y;
        }
        else
        {
            targetRootPosition.y = destination.position.y - currentHeadOffset.y;
        }

        playerRoot.position = targetRootPosition;

        if (rootRigidbody != null)
        {
            rootRigidbody.linearVelocity = Vector3.zero;
            rootRigidbody.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();

        if (characterController != null)
            characterController.enabled = true;
    }
}