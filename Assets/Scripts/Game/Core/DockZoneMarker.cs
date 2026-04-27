using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DockZoneMarker : MonoBehaviour
{
    private void Reset()
    {
        Collider zone = GetComponent<Collider>();
        zone.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        LootItem loot = other.GetComponentInParent<LootItem>();
        if (loot == null)
        {
            return;
        }

        loot.SetDocked(true);
    }

    private void OnTriggerExit(Collider other)
    {
        LootItem loot = other.GetComponentInParent<LootItem>();
        if (loot == null)
        {
            return;
        }

        loot.SetDocked(false);
    }
}
