using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    public float scrollSpeed = 30f;
    public GameObject StateBar; // Correctly typed as GameObject
    private CanvasGroup stateBarCanvasGroup;

    void Start()
    {
        if (StateBar != null)
        {
            StateBar.SetActive(false); // Ensure StateBar is initially inactive
            stateBarCanvasGroup = StateBar.GetComponent<CanvasGroup>();
            if (stateBarCanvasGroup == null)
            {
                stateBarCanvasGroup = StateBar.AddComponent<CanvasGroup>();
            }
            stateBarCanvasGroup.alpha = 0;
            StartCoroutine(ActivateStateBarAfterDelay(90f)); // Start coroutine to activate StateBar after 90 seconds
        }
        else
        {
            Debug.LogWarning("StateBar is not assigned in the inspector.");
        }
    }

    void Update()
    {
        // Moves the object upward in world space at a defined scroll speed
        transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);
    }

    IEnumerator ActivateStateBarAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (StateBar != null)
        {
            StateBar.SetActive(true); // Activate StateBar
            StartCoroutine(FadeIn(stateBarCanvasGroup, 1f)); // Fade in over 1 second
        }
    }

    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, time / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public void Home()
    {
        // Implement Home button functionality to go to the start screen
        SceneManager.LoadScene("1StartScreen"); // Replace "StartScreen" with the actual name of your start screen scene
        Debug.Log("Home button pressed.");
    }
}
