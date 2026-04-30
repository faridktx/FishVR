using UnityEngine;

public class NetReturnController : MonoBehaviour
{
    public float baseReturnSpeed = 8f;
    public float minimumReturnSpeed = 1.5f;
    public float turningSpeed = 10f;
    public float slowdownPerWeight = 0.25f;

    [Header("Audio")]
    public AudioSource reelAudioSource;
    public AudioClip reelLoopClip;

    public bool IsReturning { get; private set; }

    private Transform dockTarget;
    private MagnetCollector magnetCollector;

    public void Initialize(Transform targetDock, MagnetCollector collector)
    {
        dockTarget = targetDock;
        magnetCollector = collector;
    }

    public void BeginReturn()
    {
        IsReturning = true;
        StartReelLoop();
    }

    public void StopReturn()
    {
        IsReturning = false;
        StopReelLoop();
    }

    public float GetCurrentReturnSpeed()
    {
        float weight = magnetCollector != null ? magnetCollector.GetTotalWeight() : 0f;
        float speed = baseReturnSpeed - (weight * slowdownPerWeight);
        return Mathf.Max(minimumReturnSpeed, speed);
    }

    private void Update()
    {
        if (!IsReturning || dockTarget == null)
        {
            return;
        }

        Vector3 direction = (dockTarget.position - transform.position);
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 forward = direction.normalized;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(forward),
            Time.deltaTime * turningSpeed
        );

        transform.position += forward * GetCurrentReturnSpeed() * Time.deltaTime;
    }

    private void OnDisable()
    {
        StopReelLoop();
    }

    private void StartReelLoop()
    {
        AudioSource source = reelAudioSource != null ? reelAudioSource : GetComponent<AudioSource>();
        if (source == null || reelLoopClip == null)
        {
            return;
        }

        source.loop = true;
        source.clip = reelLoopClip;

        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    private void StopReelLoop()
    {
        AudioSource source = reelAudioSource != null ? reelAudioSource : GetComponent<AudioSource>();
        if (source == null)
        {
            return;
        }

        if (source.isPlaying && source.clip == reelLoopClip)
        {
            source.Stop();
        }

        if (source.clip == reelLoopClip)
        {
            source.clip = null;
        }

        source.loop = false;
    }
}
