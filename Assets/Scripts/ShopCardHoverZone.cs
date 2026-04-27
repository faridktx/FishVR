using UnityEngine;

public class ShopCardHoverZone : MonoBehaviour
{
    [SerializeField] private ShopCardVisual cardVisual;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.Contains("Poke Interactor")) return;
        cardVisual.SetHover();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.Contains("Poke Interactor")) return;
        cardVisual.SetNormal();
    }
}