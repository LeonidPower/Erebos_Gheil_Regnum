using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsFade : MonoBehaviour
{
    public CanvasGroup creditsCanvasGroup;
    public float delay = 5f;        // Time in seconds before the fade-out starts
    public float fadeDuration = 2f; // Duration of the fade-out effect

    private void Start()
    {
        // Start the FadeOut coroutine
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Gradually fade out
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            creditsCanvasGroup.alpha = 1 - (timer / fadeDuration);
            yield return null; // Wait until the next frame
        }

        // Ensure the alpha is set to 0 after fading
        creditsCanvasGroup.alpha = 0;

        // Optionally, you can disable the GameObject or perform other actions after fading out
        // gameObject.SetActive(false);
    }
}
