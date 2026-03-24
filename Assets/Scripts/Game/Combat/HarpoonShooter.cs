using UnityEngine;
using UnityEngine.InputSystem;

public class HarpoonShooter : MonoBehaviour
{
    public GameManager gameManager;
    public RunStats runStats;

    [Header("Projectile")]
    public NetProjectile projectilePrefab;
    public Transform firePoint;
    public float shootForce = 20f;
    public float maxAimDistance = 100f;

    [Header("Aim")]
    public Camera aimCamera;
    public LayerMask aimMask = ~0;

    private void Reset()
    {
        aimCamera = Camera.main;
    }

    private void Update()
    {
        if (gameManager == null || runStats == null)
        {
            return;
        }

        if (gameManager.CurrentPhase != GamePhase.AimShoot)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryShoot();
        }
    }

    public void TryShoot()
    {
        if (projectilePrefab == null || firePoint == null || runStats.ammo <= 0)
        {
            return;
        }

        Vector3 shootDirection = GetAimDirection();

        NetProjectile projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        projectile.Initialize(gameManager, shootDirection * shootForce);
        runStats.ConsumeAmmo(1);
        gameManager.OnShotFired(projectile);
    }

    private Vector3 GetAimDirection()
    {
        if (aimCamera == null)
        {
            return firePoint.forward;
        }

        Vector2 mousePosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Ray ray = aimCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask))
        {
            return (hit.point - firePoint.position).normalized;
        }

        return ray.direction.normalized;
    }
}
