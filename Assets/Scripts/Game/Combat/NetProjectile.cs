using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetProjectile : MonoBehaviour
{
    public Transform visualNet;
    public float expandSpeed = 5f;
    public Vector3 startScale = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 targetScale = new Vector3(0.4f, 0.4f, 0.4f);
    public float autoReturnDelay = 0.15f;
    public float forceLandingAfterSeconds = 12f;

    [Header("Composition")]
    public MagnetCollector magnetCollector;
    public NetReturnController returnController;

    private Rigidbody rb;
    private GameManager gameManager;
    private bool hasImpacted;

    public MagnetCollector MagnetCollector => magnetCollector;
    public NetReturnController ReturnController => returnController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (visualNet != null)
        {
            visualNet.localScale = startScale;
        }
    }

    private void Update()
    {
        if (visualNet != null)
        {
            visualNet.localScale = Vector3.Lerp(
                visualNet.localScale,
                targetScale,
                Time.deltaTime * expandSpeed
            );
        }
    }

    public void Initialize(GameManager manager, Vector3 initialVelocity)
    {
        gameManager = manager;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearVelocity = initialVelocity;
            rb.angularVelocity = Vector3.zero;
        }

        CancelInvoke(nameof(ForceLandingFailsafe));
        Invoke(nameof(ForceLandingFailsafe), Mathf.Max(1f, forceLandingAfterSeconds));
    }

    public void StartReturnToDock(Transform dockTarget)
    {
        if (returnController == null)
        {
            return;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
        }

        returnController.Initialize(dockTarget, magnetCollector);
        returnController.BeginReturn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryBeginAutoReturn();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryBeginAutoReturn();
    }

    private void TryBeginAutoReturn()
    {
        if (hasImpacted || gameManager == null)
        {
            return;
        }

        hasImpacted = true;
        Invoke(nameof(NotifyReturnStart), autoReturnDelay);
    }

    private void NotifyReturnStart()
    {
        if (gameManager != null)
        {
            gameManager.OnProjectileRequestReturn(this);
        }
    }

    private void ForceLandingFailsafe()
    {
        if (gameManager != null)
        {
            gameManager.ForceDockLanding(this);
        }
    }
}
