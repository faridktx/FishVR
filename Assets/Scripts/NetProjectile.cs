using UnityEngine;

public class NetProjectile : MonoBehaviour
{
    public Transform visualNet;
    public float expandSpeed = 5f;
    public Vector3 startScale = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 targetScale = new Vector3(0.4f, 0.4f, 0.4f);

    void Start()
    {
        if (visualNet != null)
        {
            visualNet.localScale = startScale;
        }
    }

    void Update()
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
}