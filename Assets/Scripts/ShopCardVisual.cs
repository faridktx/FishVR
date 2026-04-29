using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardVisual : MonoBehaviour
{
    [SerializeField] private string powerupName = "Long Range";

    [Header("Visual Feedback")]
    [SerializeField] private float hoverScale = 1.03f;
    [SerializeField] private float pressedScale = 0.98f;
    [SerializeField] private float animationTime = 0.12f;

    private Vector3 originalScale;
    private Image cardImage;
    private Coroutine scaleRoutine;

    private Color normalColor = new Color32(81, 142, 98, 255);
    private Color hoverColor = new Color32(108, 175, 122, 255);
    private Color pressedColor = new Color32(62, 112, 75, 255);

    private void Awake()
    {
        originalScale = transform.localScale;
        cardImage = GetComponent<Image>();
        SetNormal();
    }

    public void SetHover()
    {
        SetVisual(hoverColor, originalScale * hoverScale);
    }

    public UnityEngine.Events.UnityEvent onCardPressed;

    public void SetPressed()
    {
        Debug.Log(powerupName + " selected");
        SetVisual(pressedColor, originalScale * pressedScale);
        StartCoroutine(ReturnToHover());
        
        if (onCardPressed != null)
        {
            onCardPressed.Invoke();
        }
    }

    public void SetNormal()
    {
        SetVisual(normalColor, originalScale);
    }

    private IEnumerator ReturnToHover()
    {
        yield return new WaitForSeconds(0.18f);
        SetHover();
    }

    private void SetVisual(Color color, Vector3 scale)
    {
        if (cardImage != null)
            cardImage.color = color;

        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleTo(scale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float timer = 0f;

        while (timer < animationTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, timer / animationTime);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
