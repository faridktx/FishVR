using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterFloat : MonoBehaviour
{
    [Header("Water Surface")]
    public float waterLevelY = 0f;
    public float buoyancyForce = 18f;
    public float verticalDamping = 1.8f;
    public float maxDownwardBuoyancy = 8f;
    [Range(0f, 1.5f)] public float gravityCompensation = 1f;
    public float surfaceFollowStrength = 1f;

    [Header("Bobbing")]
    public float bobAmplitude = 0.06f;
    public float bobFrequency = 0.9f;
    public float secondaryBobAmplitude = 0.025f;
    public float secondaryBobFrequency = 1.7f;
    public float bobNoiseAmplitude = 0.012f;
    public float bobNoiseFrequency = 0.55f;
    public float bobPhaseOffset;

    [Header("Rotation Stabilization")]
    public float uprightStrength = 5.5f;
    public float angularDamping = 2.2f;
    public float maxAngularSpeedDegrees = 70f;

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

        float t = Time.time + bobPhaseOffset;
        float primaryBob = Mathf.Sin(t * bobFrequency) * bobAmplitude;
        float secondaryBob = Mathf.Sin(t * secondaryBobFrequency + 1.7f) * secondaryBobAmplitude;
        float noise = (Mathf.PerlinNoise(t * bobNoiseFrequency, bobPhaseOffset) - 0.5f) * 2f * bobNoiseAmplitude;
        float bob = primaryBob + secondaryBob + noise;
        float targetY = waterLevelY + bob;
        float verticalOffset = targetY - rb.position.y;
        float gravityLift = rb.useGravity ? -Physics.gravity.y * gravityCompensation : 0f;
        float buoyancyAcceleration = gravityLift + (verticalOffset * buoyancyForce * surfaceFollowStrength) - (rb.linearVelocity.y * verticalDamping);
        buoyancyAcceleration = Mathf.Clamp(buoyancyAcceleration, -maxDownwardBuoyancy, buoyancyForce);
        rb.AddForce(Vector3.up * buoyancyAcceleration, ForceMode.Acceleration);

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

        float maxAngularSpeed = maxAngularSpeedDegrees * Mathf.Deg2Rad;
        if (rb.angularVelocity.sqrMagnitude > maxAngularSpeed * maxAngularSpeed)
        {
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
        }
    }
}
