using UnityEngine;

[System.Serializable]
public class backgroundparallax : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private SpriteRenderer[] layers;
    public float[] speeds;

    private UnityEngine.Vector3 previousCamPos;

    void Start()
    {
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (cam != null)
        {
            previousCamPos = cam.position;
        }
    }

    private void LateUpdate()
    {
        if (cam == null || layers == null || speeds == null)
        {
            return;
        }

        UnityEngine.Vector3 camPos = cam.position;
        UnityEngine.Vector3 delta = camPos - previousCamPos;

        int count = Mathf.Min(layers.Length, speeds.Length);
        for (int i = 0; i < count; i++)
        {
            if (layers[i] == null) continue;

            Transform layerTransform = layers[i].transform;
            layerTransform.position += new UnityEngine.Vector3(delta.x * speeds[i], delta.y * speeds[i], 0f);
        }

        previousCamPos = camPos;
    }
}
