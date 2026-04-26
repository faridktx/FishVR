using UnityEngine;

public class ShopCardPressZone : MonoBehaviour
{
    [SerializeField] private ShopCardVisual cardVisual;

    private bool hasPressed = false;
    private float nextPressTime = 0f;

    [SerializeField] private float pressCooldown = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.Contains("Poke Interactor")) return;
        if (hasPressed) return;
        if (Time.time < nextPressTime) return;

        hasPressed = true;
        nextPressTime = Time.time + pressCooldown;

        cardVisual.SetPressed();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.Contains("Poke Interactor")) return;

        hasPressed = false;
    }
}