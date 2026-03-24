using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrashBinSeller : MonoBehaviour
{
    public GameManager gameManager;
    public RunStats runStats;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        LootItem loot = other.GetComponentInParent<LootItem>();
        if (loot == null)
        {
            return;
        }

        if (gameManager == null || runStats == null)
        {
            return;
        }

        runStats.AddCoins(Mathf.Max(0, loot.sellValue));
        gameManager.RemoveLandedItem(loot);

        SortableItem sortable = loot.GetComponent<SortableItem>();
        if (sortable != null)
        {
            sortable.Release();
        }

        Destroy(loot.gameObject);

        if (gameManager.LandedItems.Count == 0)
        {
            gameManager.OnSortingFinished();
        }
    }
}
