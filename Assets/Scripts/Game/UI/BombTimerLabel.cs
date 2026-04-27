using TMPro;
using UnityEngine;

public class BombTimerLabel : MonoBehaviour
{
    public BombDefuseController defuseController;
    public LootItem ownerBomb;
    public TMP_Text timerText;
    public Camera lookCamera;
    public string prefix = "";

    private void Awake()
    {
        if (ownerBomb == null)
        {
            ownerBomb = GetComponent<LootItem>();
        }

        if (timerText == null)
        {
            timerText = GetComponentInChildren<TMP_Text>(true);
        }

        if (defuseController == null)
        {
            defuseController = FindFirstObjectByType<BombDefuseController>();
        }

        if (timerText != null)
        {
            timerText.text = string.Empty;
            timerText.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (timerText == null || defuseController == null || ownerBomb == null)
        {
            return;
        }

        LootItem activeBomb = defuseController.ActiveBomb;
        bool show =
            defuseController.IsTimerActive &&
            activeBomb != null &&
            activeBomb == ownerBomb;

        if (!show)
        {
            if (timerText.enabled)
            {
                timerText.enabled = false;
                timerText.text = string.Empty;
            }

            return;
        }

        timerText.enabled = true;

        float remaining = Mathf.Max(0f, defuseController.RemainingTime);
        timerText.text = string.IsNullOrEmpty(prefix)
            ? remaining.ToString("0.0")
            : prefix + remaining.ToString("0.0");

        Camera cam = lookCamera != null ? lookCamera : Camera.main;
        if (cam != null)
        {
            Transform textTransform = timerText.transform;
            Vector3 toCamera = textTransform.position - cam.transform.position;
            if (toCamera.sqrMagnitude > 0.0001f)
            {
                textTransform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            }
        }
    }
}
