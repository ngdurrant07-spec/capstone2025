using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private Vector2 parallaxEffect = new Vector2(0.5f, 0f);
    [SerializeField] private bool infiniteX = true;
    [SerializeField] private bool infiniteY = false;

    private UnityEngine.Vector3 lastCamPos;
    private float textureUnitSizeX;
    private float textureUnitSizeY;
    private const int MAX_WRAP_STEPS_PER_FRAME = 8;

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

        UnityEngine.Vector3 delta = cam.position - lastCamPos;
        UnityEngine.Vector3 move = new UnityEngine.Vector3(delta.x * parallaxEffect.x, delta.y * parallaxEffect.y, 0f);
        transform.position += move;
        lastCamPos = cam.position;

        if (infiniteX && textureUnitSizeX > 0f)
        {
            WrapX();
        }

        if (infiniteY && textureUnitSizeY > 0f)
        {
            WrapY();
        }
    }

    private void WrapX()
    {
        int steps = 0;
        float diff = cam.position.x - transform.position.x;

        while (diff >= textureUnitSizeX && steps < MAX_WRAP_STEPS_PER_FRAME)
        {
            transform.position += new UnityEngine.Vector3(textureUnitSizeX, 0f, 0f);
            diff -= textureUnitSizeX;
            steps++;
        }

        while (diff <= -textureUnitSizeX && steps < MAX_WRAP_STEPS_PER_FRAME)
        {
            transform.position -= new UnityEngine.Vector3(textureUnitSizeX, 0f, 0f);
            diff += textureUnitSizeX;
            steps++;
        }
    }

    private void WrapY()
    {
        int steps = 0;
        float diff = cam.position.y - transform.position.y;

        while (diff >= textureUnitSizeY && steps < MAX_WRAP_STEPS_PER_FRAME)
        {
            transform.position += new UnityEngine.Vector3(0f, textureUnitSizeY, 0f);
            diff -= textureUnitSizeY;
            steps++;
        }

        while (diff <= -textureUnitSizeY && steps < MAX_WRAP_STEPS_PER_FRAME)
        {
            transform.position -= new UnityEngine.Vector3(0f, textureUnitSizeY, 0f);
            diff += textureUnitSizeY;
            steps++;
        }
    }
}
