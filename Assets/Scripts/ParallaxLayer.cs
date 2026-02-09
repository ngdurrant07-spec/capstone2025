using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private Vector2 parallaxEffect = new Vector2(0.5f, 0f);
    [SerializeField] private bool infiniteX = true;
    [SerializeField] private bool infiniteY = false;

    private Vector3 lastCamPos;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    private void Start()
    {
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (cam != null)
        {
            lastCamPos = cam.position;
        }

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var bounds = sr.bounds.size;
            textureUnitSizeX = bounds.x;
            textureUnitSizeY = bounds.y;
        }
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            return;
        }

        Vector3 delta = cam.position - lastCamPos;
        Vector3 move = new Vector3(delta.x * parallaxEffect.x, delta.y * parallaxEffect.y, 0f);
        transform.position += move;
        lastCamPos = cam.position;

        if (infiniteX && textureUnitSizeX > 0f)
        {
            float diff = cam.position.x - transform.position.x;
            if (Mathf.Abs(diff) >= textureUnitSizeX)
            {
                float offset = diff % textureUnitSizeX;
                transform.position = new Vector3(cam.position.x - offset, transform.position.y, transform.position.z);
            }
        }

        if (infiniteY && textureUnitSizeY > 0f)
        {
            float diff = cam.position.y - transform.position.y;
            if (Mathf.Abs(diff) >= textureUnitSizeY)
            {
                float offset = diff % textureUnitSizeY;
                transform.position = new Vector3(transform.position.x, cam.position.y - offset, transform.position.z);
            }
        }
    }
}
