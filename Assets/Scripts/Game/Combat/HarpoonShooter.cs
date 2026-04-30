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

    [Header("Desktop Override")]
    public bool enableDesktopOverride = false;
    public bool desktopBlocksExternalTriggers = true;

    [Header("Audio")]
    public AudioSource combatAudioSource;
    public AudioClip shotClip;

    private bool desktopShotRequested;

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

        if (!enableDesktopOverride)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            desktopShotRequested = true;
            TryShoot();
            desktopShotRequested = false;
        }
    }

    public void TryShoot()
    {
        if (enableDesktopOverride && desktopBlocksExternalTriggers && !desktopShotRequested)
        {
            return;
        }

        if (
            gameManager == null ||
            runStats == null ||
            gameManager.CurrentPhase != GamePhase.AimShoot ||
            gameManager.activeProjectile != null ||
            projectilePrefab == null ||
            firePoint == null ||
            runStats.ammo <= 0
        )
        {
            return;
        }

        Vector3 shootDirection = GetAimDirection();

        NetProjectile projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        if (projectile == null)
        {
            return;
        }

        projectile.Initialize(gameManager, shootDirection * shootForce);
        gameManager.OnShotFired(projectile);
        runStats.ConsumeAmmo(1);
        PlayCombatOneShot(shotClip);
    }

    private void PlayCombatOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource source = combatAudioSource != null ? combatAudioSource : GetComponent<AudioSource>();
        if (source != null)
        {
            source.PlayOneShot(clip);
        }
    }

    private Vector3 GetAimDirection()
    {
        if (firePoint == null)
        {
            return transform.forward;
        }

        if (enableDesktopOverride)
        {
            Camera cam = aimCamera != null ? aimCamera : Camera.main;
            if (cam != null)
            {
                Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(centerRay, out RaycastHit centerHit, maxAimDistance, aimMask))
                {
                    return (centerHit.point - firePoint.position).normalized;
                }

                return centerRay.direction.normalized;
            }
        }

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask))
        {
            return (hit.point - firePoint.position).normalized;
        }

        return firePoint.forward;
    }
}
