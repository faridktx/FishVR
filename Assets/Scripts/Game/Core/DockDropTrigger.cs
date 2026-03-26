using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DockDropTrigger : MonoBehaviour
{
    public GameManager gameManager;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameManager == null)
        {
            return;
        }

        NetProjectile projectile = other.GetComponentInParent<NetProjectile>();
        if (projectile == null)
        {
            return;
        }

        if (projectile.ReturnController == null || !projectile.ReturnController.IsReturning)
        {
            return;
        }

        gameManager.ForceDockLanding(projectile);
    }
}
