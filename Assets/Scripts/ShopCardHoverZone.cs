using UnityEngine;

public class ShopCardHoverZone : MonoBehaviour
{
    [SerializeField] private ShopCardVisual cardVisual;

    private void OnTriggerEnter(Collider other)
    {
        if (!VrProximityButton.IsInteractorCollider(other)) return;
        if (cardVisual == null) return;

        cardVisual.SetHover();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!VrProximityButton.IsInteractorCollider(other)) return;
        if (cardVisual == null) return;

        cardVisual.SetNormal();
    }
}
