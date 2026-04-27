using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class LootItem : MonoBehaviour
{
    public LootKind kind = LootKind.Junk;
    public int sellValue = 1;
    public float weight = 1f;

    [HideInInspector] public bool isAttachedToNet;
    [HideInInspector] public bool isDocked;

    [Header("Attached Follow")]
    public float followStrength = 45f;
    public float followDamping = 8f;
    public float rotateStrength = 10f;
    public float maxFollowSpeed = 8f;

    private Rigidbody rb;
    private Collider itemCollider;
    private Transform followTarget;
    private Vector3 followLocalOffset;
    private readonly List<Collider> ignoredColliders = new List<Collider>();
    private Coroutine tableDropRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if (!isAttachedToNet || rb == null || followTarget == null || rb.isKinematic)
        {
            return;
        }

        Vector3 targetPos = followTarget.TransformPoint(followLocalOffset);
        Vector3 toTarget = targetPos - rb.position;
        Vector3 force = toTarget * followStrength - rb.linearVelocity * followDamping;
        rb.AddForce(force, ForceMode.Acceleration);

        if (rb.linearVelocity.sqrMagnitude > maxFollowSpeed * maxFollowSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxFollowSpeed;
        }

        Quaternion targetRot = followTarget.rotation;
        Quaternion delta = targetRot * Quaternion.Inverse(rb.rotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f)
        {
            angle -= 360f;
        }

        if (axis.sqrMagnitude > 0.0001f && Mathf.Abs(angle) > 0.05f)
        {
            Vector3 torque = axis.normalized * (angle * Mathf.Deg2Rad * rotateStrength) - rb.angularVelocity * 0.75f;
            rb.AddTorque(torque, ForceMode.Acceleration);
        }
    }

    public void AttachTo(Transform target, Vector3 localOffset)
    {
        isDocked = false;
        isAttachedToNet = true;
        followTarget = target;
        followLocalOffset = localOffset;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        SetIgnoreCollisionWithTarget(true);
    }

    public void DetachFromNet()
    {
        isAttachedToNet = false;
        followTarget = null;
        followLocalOffset = Vector3.zero;
        SetIgnoreCollisionWithTarget(false);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
    }

    public void PlaceOnTable(Vector3 worldPosition, float holdDuration, float releaseDownwardSpeed)
    {
        isDocked = true;

        if (tableDropRoutine != null)
        {
            StopCoroutine(tableDropRoutine);
        }

        tableDropRoutine = StartCoroutine(TableDropRoutine(worldPosition, holdDuration, releaseDownwardSpeed));
    }

    private IEnumerator TableDropRoutine(Vector3 worldPosition, float holdDuration, float releaseDownwardSpeed)
    {
        transform.position = worldPosition;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        yield return new WaitForSeconds(Mathf.Max(0f, holdDuration));

        if (isAttachedToNet)
        {
            tableDropRoutine = null;
            yield break;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.down * Mathf.Max(0f, releaseDownwardSpeed);
        }

        tableDropRoutine = null;
    }

    public void SetDocked(bool value)
    {
        isDocked = value;
    }

    private void SetIgnoreCollisionWithTarget(bool ignore)
    {
        if (itemCollider == null)
        {
            return;
        }

        if (ignore)
        {
            ignoredColliders.Clear();
            if (followTarget == null)
            {
                return;
            }

            Collider[] targetColliders = followTarget.GetComponentsInChildren<Collider>();
            for (int i = 0; i < targetColliders.Length; i++)
            {
                Collider c = targetColliders[i];
                if (c == null || c == itemCollider)
                {
                    continue;
                }

                Physics.IgnoreCollision(itemCollider, c, true);
                ignoredColliders.Add(c);
            }

            return;
        }

        for (int i = ignoredColliders.Count - 1; i >= 0; i--)
        {
            Collider c = ignoredColliders[i];
            if (c == null)
            {
                continue;
            }

            Physics.IgnoreCollision(itemCollider, c, false);
        }

        ignoredColliders.Clear();
    }
}
