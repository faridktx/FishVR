using System.Collections.Generic;
using UnityEngine;

public class MagnetCollector : MonoBehaviour
{
    public Transform attachPivot;
    public float magnetRadius = 1.5f;
    public LayerMask lootMask = ~0;
    public int maxAttachedItems = 24;

    [Header("Attachment Shape")]
    public float pullTowardCenter = 0.35f;
    public float tangentialSpread = 0.2f;
    public float verticalSpread = 0.15f;

    private readonly List<LootItem> attachedItems = new List<LootItem>();

    public IReadOnlyList<LootItem> AttachedItems => attachedItems;

    private void Update()
    {
        CollectNearbyLoot();
    }

    public float GetTotalWeight()
    {
        float total = 0f;
        for (int i = attachedItems.Count - 1; i >= 0; i--)
        {
            LootItem item = attachedItems[i];
            if (item == null)
            {
                attachedItems.RemoveAt(i);
                continue;
            }

            total += Mathf.Max(0f, item.weight);
        }

        return total;
    }

    public List<LootItem> DetachAll()
    {
        List<LootItem> detached = new List<LootItem>();

        for (int i = attachedItems.Count - 1; i >= 0; i--)
        {
            LootItem item = attachedItems[i];
            if (item == null)
            {
                continue;
            }

            item.DetachFromNet();
            detached.Add(item);
        }

        attachedItems.Clear();
        return detached;
    }

    private void CollectNearbyLoot()
    {
        if (attachPivot == null)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, magnetRadius, lootMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            if (attachedItems.Count >= maxAttachedItems)
            {
                return;
            }

            LootItem item = hits[i].GetComponentInParent<LootItem>();
            if (item == null || item.isAttachedToNet || item.isDocked)
            {
                continue;
            }

            Vector3 localAttachOffset = CalculateAttachOffset(item.transform.position);
            item.AttachTo(attachPivot, localAttachOffset);
            attachedItems.Add(item);
        }
    }

    private Vector3 CalculateAttachOffset(Vector3 itemWorldPosition)
    {
        if (attachPivot == null)
        {
            return Vector3.zero;
        }

        Vector3 toItem = itemWorldPosition - attachPivot.position;
        if (toItem.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        Vector3 dir = toItem.normalized;
        Vector3 inward = -dir * pullTowardCenter;
        Vector3 tangent = Vector3.Cross(dir, Vector3.up);
        if (tangent.sqrMagnitude < 0.0001f)
        {
            tangent = Vector3.Cross(dir, Vector3.right);
        }

        tangent.Normalize();

        Vector3 worldOffset = toItem
            + inward
            + tangent * Random.Range(-tangentialSpread, tangentialSpread)
            + Vector3.up * Random.Range(-verticalSpread, verticalSpread);

        return attachPivot.InverseTransformVector(worldOffset);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}
