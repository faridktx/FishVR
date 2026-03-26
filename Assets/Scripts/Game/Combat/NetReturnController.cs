using UnityEngine;

public class NetReturnController : MonoBehaviour
{
    public float baseReturnSpeed = 8f;
    public float minimumReturnSpeed = 1.5f;
    public float turningSpeed = 10f;
    public float slowdownPerWeight = 0.25f;

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
    }

    public void StopReturn()
    {
        IsReturning = false;
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
}
