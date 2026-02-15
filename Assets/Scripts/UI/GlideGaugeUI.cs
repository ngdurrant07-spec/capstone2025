using UnityEngine;
using UnityEngine.UI;

public class GlideGaugeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerScript player;
    [SerializeField] private Image gaugeFillImage;

    [Header("Behavior")]
    [SerializeField] private bool autoFindPlayer = true;

    void Awake()
    {
        if (gaugeFillImage == null)
            gaugeFillImage = GetComponent<Image>();
    }

    void Start()
    {
        TryFindPlayer();
        UpdateGauge();
    }

    void Update()
    {
        if (player == null && autoFindPlayer)
            TryFindPlayer();

        UpdateGauge();
    }

    private void TryFindPlayer()
    {
        if (!autoFindPlayer || player != null)
            return;

        player = FindFirstObjectByType<PlayerScript>();
    }

    private void UpdateGauge()
    {
        if (gaugeFillImage == null)
            return;

        gaugeFillImage.fillAmount = player != null ? player.GlideEnergyNormalized : 0f;
    }
}
