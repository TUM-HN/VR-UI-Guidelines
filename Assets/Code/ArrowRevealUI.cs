using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ArrowFade : MonoBehaviour
{
    private Image arrowImage;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        arrowImage = GetComponent<Image>();
        if (arrowImage != null)
        {
            arrowImage.fillAmount = 0f;
        }
    }

    public IEnumerator PlayFade(float duration, float holdTime)
    {
        if (arrowImage == null)
            yield break;

        // Stop any existing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeSequence(duration, holdTime));
        yield return fadeCoroutine;
    }

    public void StopFade()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        // Immediately reset to invisible state
        if (arrowImage != null)
        {
            arrowImage.fillAmount = 0f;
        }
    }

    private IEnumerator FadeSequence(float duration, float holdTime)
    {
        float elapsed = 0f;

        // Fade in bottom to top
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            arrowImage.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        // Fade out top to bottom
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            arrowImage.fillAmount = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }

        arrowImage.fillAmount = 0f; // Fully hidden again
        fadeCoroutine = null;
    }
}