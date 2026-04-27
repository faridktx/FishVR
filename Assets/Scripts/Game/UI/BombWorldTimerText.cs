using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class BombWorldTimerText : MonoBehaviour
{
    public BombDefuseController defuseController;
    public LootItem ownerBomb;
    public Camera lookCamera;
    public string prefix = "";

    private TMP_Text timerText;

    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();

        if (ownerBomb == null)
        {
            ownerBomb = GetComponentInParent<LootItem>();
        }
    }

    private void LateUpdate()
    {
        if (timerText == null || defuseController == null)
        {
            return;
        }

        LootItem activeBomb = defuseController.ActiveBomb;
        bool show =
            defuseController.IsTimerActive &&
            activeBomb != null &&
            ownerBomb != null &&
            activeBomb == ownerBomb;

        if (!show)
        {
            timerText.enabled = false;
            return;
        }

        timerText.enabled = true;

        Camera cam = lookCamera != null ? lookCamera : Camera.main;
        if (cam != null)
        {
            Vector3 toCamera = transform.position - cam.transform.position;
            if (toCamera.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            }
        }

        float remaining = Mathf.Max(0f, defuseController.RemainingTime);
        timerText.text = string.IsNullOrEmpty(prefix)
            ? remaining.ToString("0.0")
            : prefix + remaining.ToString("0.0");
    }
}
