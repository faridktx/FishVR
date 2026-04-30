using UnityEngine;

public class ShopCardPressZone : MonoBehaviour
{
    [SerializeField] private ShopCardVisual cardVisual;

    private bool hasPressed = false;
    private float nextPressTime = 0f;

    [SerializeField] private float pressCooldown = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        TryPress(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryPress(other);
    }

    private void TryPress(Collider other)
    {
        if (!VrProximityButton.IsInteractorCollider(other)) return;
        if (cardVisual == null) return;
        if (hasPressed) return;
        if (Time.time < nextPressTime) return;

        hasPressed = true;
        nextPressTime = Time.time + pressCooldown;

        cardVisual.SetPressed();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!VrProximityButton.IsInteractorCollider(other)) return;

        hasPressed = false;
    }
}
