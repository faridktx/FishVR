using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StreamCurrentController : MonoBehaviour
{
    public Vector3 flowDirection = Vector3.right;
    public float flowSpeed = 2.5f;
    public float upwardBias = 0f;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        LootItem item = other.GetComponentInParent<LootItem>();
        if (item == null || item.isAttachedToNet)
        {
            return;
        }

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic)
        {
            return;
        }

        Vector3 dir = flowDirection.sqrMagnitude > 0.0001f ? flowDirection.normalized : Vector3.right;
        Vector3 targetVelocity = dir * flowSpeed;
        targetVelocity.y += upwardBias;

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Bounds b = GetComponent<Collider>() != null ? GetComponent<Collider>().bounds : new Bounds(transform.position, Vector3.one);
        Gizmos.DrawWireCube(b.center, b.size);

        Vector3 dir = flowDirection.sqrMagnitude > 0.0001f ? flowDirection.normalized : Vector3.right;
        Vector3 start = transform.position;
        Gizmos.DrawLine(start, start + dir * 2f);
    }
}
