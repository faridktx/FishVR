using UnityEngine;

[DisallowMultipleComponent]
public class WaterProximityAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource waterAudioSource;
    [SerializeField] private Transform listenerTransform;

    [Header("Distance")]
    [SerializeField, Min(0.1f)] private float nearDistance = 4f;
    [SerializeField, Min(0.1f)] private float farDistance = 12f;

    [Header("Fade")]
    [SerializeField, Min(0f)] private float fadeSpeed = 3f;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 0.6f;

    private float targetVolume;

    private void Awake()
    {
        if (waterAudioSource == null)
        {
            waterAudioSource = GetComponent<AudioSource>();
        }

        if (waterAudioSource != null)
        {
            waterAudioSource.loop = true;
            waterAudioSource.playOnAwake = false;
            waterAudioSource.volume = 0f;
        }

        if (listenerTransform == null && Camera.main != null)
        {
            listenerTransform = Camera.main.transform;
        }

        if (farDistance < nearDistance)
        {
            farDistance = nearDistance;
        }
    }

    private void Update()
    {
        if (listenerTransform == null && Camera.main != null)
        {
            listenerTransform = Camera.main.transform;
        }

        if (waterAudioSource == null || listenerTransform == null)
        {
            return;
        }

        float distance = Vector3.Distance(listenerTransform.position, transform.position);
        float t = Mathf.InverseLerp(farDistance, nearDistance, distance);
        targetVolume = t * maxVolume;

        if (targetVolume > 0.001f && !waterAudioSource.isPlaying)
        {
            waterAudioSource.Play();
        }

        waterAudioSource.volume = Mathf.MoveTowards(
            waterAudioSource.volume,
            targetVolume,
            fadeSpeed * Time.deltaTime);

        if (waterAudioSource.isPlaying && waterAudioSource.volume <= 0.001f && targetVolume <= 0.001f)
        {
            waterAudioSource.Stop();
        }
    }
}
