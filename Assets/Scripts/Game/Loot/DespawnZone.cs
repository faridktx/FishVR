using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DespawnZone : MonoBehaviour
{
    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        LootItem item = other.GetComponentInParent<LootItem>();
        if (item == null)
        {
            return;
        }

        Destroy(item.gameObject);
    }
}
