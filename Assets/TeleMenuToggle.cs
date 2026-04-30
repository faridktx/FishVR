using UnityEngine;
using UnityEngine.InputSystem;

public class TeleMenuToggle : MonoBehaviour
{
    public InputActionReference menuButton;
    public GameObject menuCanvas;
    public Collider[] hitboxColliders;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip menuEnableClip;
    public AudioClip menuDisableClip;

    private void Start()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(false);
        foreach (Collider col in hitboxColliders)
            if (col != null) col.enabled = false;
    }

    private void OnEnable()
    {
        if (menuButton != null && menuButton.action != null)
            menuButton.action.Enable();
    }

    private void OnDisable()
    {
        if (menuButton != null && menuButton.action != null)
            menuButton.action.Disable();
    }

    private void Update()
    {
        if (menuButton == null || menuButton.action == null || menuCanvas == null) return;

        if (menuButton.action.WasPressedThisFrame())
        {
            bool newState = !menuCanvas.activeSelf;
            menuCanvas.SetActive(newState);
            foreach (Collider col in hitboxColliders)
                if (col != null) col.enabled = newState;

            PlayUiOneShot(newState ? menuEnableClip : menuDisableClip);
        }
    }

    private void PlayUiOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource source = uiAudioSource != null ? uiAudioSource : GetComponent<AudioSource>();
        if (source != null)
        {
            source.PlayOneShot(clip);
        }
    }
}