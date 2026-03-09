using UnityEngine;

public class HarpoonLauncher : MonoBehaviour
{
    public GameObject netProjectilePrefab;
    public Transform firePoint;
    public float shootForce = 10f;
    public float destroyAfterSeconds = 5f;

    public void Shoot()
    {
        if (netProjectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("HarpoonLauncher is missing references.");
            return;
        }

        GameObject net = Instantiate(
            netProjectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = net.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(firePoint.forward * shootForce, ForceMode.Impulse);
        }

        // Destroy(net, destroyAfterSeconds);

        Debug.Log("Net fired!");
    }
}