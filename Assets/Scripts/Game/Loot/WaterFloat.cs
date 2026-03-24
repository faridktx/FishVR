using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterFloat : MonoBehaviour
{
    [Header("Water Surface")]
    public float waterLevelY = 0f;
    public float buoyancyForce = 18f;
    public float verticalDamping = 1.8f;

    [Header("Bobbing")]
    public float bobAmplitude = 0.08f;
    public float bobFrequency = 1.8f;
    public float bobPhaseOffset;

    [Header("Rotation Stabilization")]
    public float uprightStrength = 4f;
    public float angularDamping = 0.9f;

    private Rigidbody rb;
    private LootItem lootItem;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lootItem = GetComponent<LootItem>();

        if (Mathf.Approximately(bobPhaseOffset, 0f))
        {
            bobPhaseOffset = Random.Range(0f, 100f);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null || rb.isKinematic)
        {
            return;
        }

        if (lootItem != null && lootItem.isAttachedToNet)
        {
            return;
        }

        float bob = Mathf.Sin((Time.time + bobPhaseOffset) * bobFrequency) * bobAmplitude;
        float targetY = waterLevelY + bob;
        float depth = targetY - rb.position.y;

        if (depth > 0f)
        {
            Vector3 upwardForce = Vector3.up * (depth * buoyancyForce);
            Vector3 dampingForce = Vector3.down * (rb.linearVelocity.y * verticalDamping);
            rb.AddForce(upwardForce + dampingForce, ForceMode.Acceleration);
        }

        Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        Quaternion delta = targetRot * Quaternion.Inverse(rb.rotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f)
        {
            angle -= 360f;
        }

        if (Mathf.Abs(angle) > 0.01f && axis.sqrMagnitude > 0.0001f)
        {
            Vector3 torque = axis.normalized * (angle * Mathf.Deg2Rad * uprightStrength) - rb.angularVelocity * angularDamping;
            rb.AddTorque(torque, ForceMode.Acceleration);
        }
    }
}
