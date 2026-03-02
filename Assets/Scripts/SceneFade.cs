using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFade : MonoBehaviour
{
    private Image _sceneFadeImage;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _sceneFadeImage = GetComponentInChildren<Image>(true);
        _canvasGroup = GetComponentInChildren<CanvasGroup>(true);

        if (_sceneFadeImage != null)
            _sceneFadeImage.raycastTarget = false;

        SetRaycastBlocking(false);
    }

    public IEnumerator FadeInCoroutine(float duration)
    {
        if (_sceneFadeImage == null)
            yield break;

        gameObject.SetActive(true);
        SetRaycastBlocking(true);
        Color startColor = new Color(_sceneFadeImage.color.r, _sceneFadeImage.color.g, _sceneFadeImage.color.b, 1f);
        Color targetColor = new Color(_sceneFadeImage.color.r, _sceneFadeImage.color.g, _sceneFadeImage.color.b, 0f);
        yield return FadeCoroutine(startColor, targetColor, duration);

        SetRaycastBlocking(false);
        gameObject.SetActive(false);
    }

    public IEnumerator FadeOutCoroutine(float duration)
    {
        if (_sceneFadeImage == null)
            yield break;

        gameObject.SetActive(true);
        SetRaycastBlocking(true);
        Color startColor = new Color(_sceneFadeImage.color.r, _sceneFadeImage.color.g, _sceneFadeImage.color.b, 0f);
        Color targetColor = new Color(_sceneFadeImage.color.r, _sceneFadeImage.color.g, _sceneFadeImage.color.b, 1f);
        yield return FadeCoroutine(startColor, targetColor, duration);
    }

    private IEnumerator FadeCoroutine(Color startColor, Color targetColor, float duration)
    {
        if (_sceneFadeImage == null)
            yield break;

        if (duration <= 0f)
        {
            _sceneFadeImage.color = targetColor;
            yield break;
        }

        float elapsedTime = 0;
        float elapsedPercentage = 0;

        _sceneFadeImage.color = startColor;

        while (elapsedPercentage < 1)
        {
            elapsedPercentage = elapsedTime / duration;
            _sceneFadeImage.color = Color.Lerp(startColor, targetColor, elapsedPercentage);

            yield return null;
            elapsedTime += Time.deltaTime;
        }

        _sceneFadeImage.color = targetColor;
    }

    private void SetRaycastBlocking(bool shouldBlock)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = shouldBlock;
            _canvasGroup.interactable = shouldBlock;
        }
    }
}
