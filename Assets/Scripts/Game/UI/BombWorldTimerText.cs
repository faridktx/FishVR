using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class BombWorldTimerText : MonoBehaviour
{
    public BombDefuseController defuseController;
    public LootItem ownerBomb;
    public Camera lookCamera;
    public string prefix = "";
    public float rightOffset = 0.12f;
    public float upOffset = 0.12f;
    public float towardCameraOffset = 0.04f;

    private TMP_Text timerText;
    private Renderer ownerRenderer;

    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();

        if (ownerBomb == null)
        {
            ownerBomb = GetComponentInParent<LootItem>();
        }

        if (ownerBomb != null)
        {
            ownerRenderer = ownerBomb.GetComponentInChildren<Renderer>();
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
            Vector3 anchor = GetAnchorPosition();
            transform.position =
                anchor
                + cam.transform.right * rightOffset
                + cam.transform.up * upOffset
                + cam.transform.forward * towardCameraOffset;

            Vector3 toCamera = cam.transform.position - transform.position;
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

    private Vector3 GetAnchorPosition()
    {
        if (ownerRenderer == null && ownerBomb != null)
        {
            ownerRenderer = ownerBomb.GetComponentInChildren<Renderer>();
        }

        if (ownerRenderer != null)
        {
            return ownerRenderer.bounds.center;
        }

        if (ownerBomb != null)
        {
            return ownerBomb.transform.position;
        }

        return transform.position;
    }
}
