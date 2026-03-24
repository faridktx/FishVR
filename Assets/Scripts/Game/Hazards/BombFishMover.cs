using UnityEngine;

public class BombFishMover : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float turnSpeed = 6f;

    private bool movingToB = true;

    private void Update()
    {
        if (pointA == null || pointB == null)
        {
            return;
        }

        Vector3 target = movingToB ? pointB.position : pointA.position;
        Vector3 delta = target - transform.position;

        if (delta.sqrMagnitude <= 0.1f * 0.1f)
        {
            movingToB = !movingToB;
            return;
        }

        Vector3 dir = delta.normalized;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * turnSpeed
        );
        transform.position += dir * speed * Time.deltaTime;
    }
}
