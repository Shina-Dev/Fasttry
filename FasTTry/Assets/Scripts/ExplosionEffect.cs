using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private float duration = 0.4f;
    private float elapsed = 0f;
    private float maxScale = 3f;
    private MeshRenderer mr;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        elapsed = 0f;
        transform.localScale = Vector3.zero;
        if (mr != null)
            mr.material.color = new Color(1f, 0.3f, 0f, 0.8f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / duration;

        float scale = Mathf.Lerp(0f, maxScale, progress);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (mr != null)
            mr.material.color = new Color(1f, 0.3f, 0f, Mathf.Lerp(0.8f, 0f, progress));

        if (elapsed >= duration)
            Destroy(gameObject);
    }
}