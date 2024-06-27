using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using System.Collections;

public class Name_Input : MonoBehaviour
{
    public TMP_InputField nameInputField; // Attach your TMP_InputField here in the inspector
    public TMP_Text feedbackText; // Attach a TMP_Text to show feedback to the user
    public Button confirmButton; // Reference to the confirm button
    public CanvasGroup fadePanel; // Assign your full-screen UI Panel's CanvasGroup here
    public string nextSceneName = "3Conversation"; // The name of the scene to load

     public CUIColorPicker colorPicker; // Attach your CUIColorPicker here in the inspector
    public TMP_Text titleText; // Attach your Title TMP_Text here in the inspector
    private Color selectedColor; // To store the selected color
    public Button colorButton;
    public CanvasGroup colorPickerCanvasGroup,TextCanvasGroup;

    void Start()
    {
       

        colorPicker.gameObject.SetActive(false);
        colorPicker.SetOnValueChangeCallback(OnColorChange);
        // Set up the confirm button event listener here
        confirmButton.onClick.AddListener(OnConfirmClicked);
        colorButton.onClick.AddListener(ToggleColorPicker);
        // Start the fade effect
        StartCoroutine(FadeScreenIn());
    }
    private void OnColorChange(Color newColor)
{
     colorButton.image.color = newColor;
    // Apply the new color to the input field text
    nameInputField.textComponent.color = newColor;
}

 public void ToggleColorPicker()
{
    // Check if the color picker is currently visible by checking the alpha
    bool isActive = colorPickerCanvasGroup.alpha > 0;
    if (!isActive)
    {
        // Fade in
        colorPicker.gameObject.SetActive(true); // Ensure the GameObject is active to fade in
        StartCoroutine(FadeColorPicker(true));
    }
    else
    {
        // Fade out
        StartCoroutine(FadeColorPicker(false));
    }
}

private IEnumerator FadeColorPicker(bool fadeIn)
{
    float startAlpha = fadeIn ? 0f : 1f;
    float endAlpha = fadeIn ? 1f : 0f;
    float duration = 1.0f;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        // Update elapsed time
        elapsed += Time.deltaTime;
        // Update alpha based on the elapsed time
        colorPickerCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
        yield return null;
    }

    // Ensure the final alpha is exactly the target alpha
    colorPickerCanvasGroup.alpha = endAlpha;

    // Deactivate the GameObject after fading out
    if (!fadeIn)
    {
        colorPicker.gameObject.SetActive(false);
    }
}



    public void OnConfirmClicked()
{
    string playerName = nameInputField.text;

    // Check if the input is "LeonidPower" and give specific feedback
    if (playerName.Equals("LeonidPower", System.StringComparison.OrdinalIgnoreCase))
    {
        if (feedbackText != null)
        {
            ShowFeedbackMessage("There is only one King");
        }
        // Optionally, you could return here if you don't want to proceed further
        return;
    }

    // Validate the player's name length
    if (!string.IsNullOrEmpty(playerName) && playerName.Length >= 1 && playerName.Length <= 15)
    {
        // Save the name to persistent data if you want to use it in the next scene
        PlayerPrefs.SetString("PlayerName", playerName);
        // Start the fade-out effect, then load the new scene
        selectedColor = colorPicker.Color;

        // Save the RGBA components of the color to PlayerPrefs
        PlayerPrefs.SetFloat("PlayerColorR", selectedColor.r);
        PlayerPrefs.SetFloat("PlayerColorG", selectedColor.g);
        PlayerPrefs.SetFloat("PlayerColorB", selectedColor.b);
        PlayerPrefs.SetFloat("PlayerColorA", selectedColor.a);

        // Save PlayerPrefs changes
        PlayerPrefs.Save();

        StartCoroutine(FadeScreenOut(nextSceneName));
    }
    else
    {
        // Provide feedback to the player that the input is not valid
        if (feedbackText != null)
        {
        
            ShowFeedbackMessage("Name must be between 1 and 15 characters long");
            
        }
    }
}


    // Coroutine to fade the screen in
    IEnumerator FadeScreenIn()
    {
        fadePanel.gameObject.SetActive(true);
        float duration = 1.0f;
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            fadePanel.alpha = 1 - currentTime / duration;
            yield return null;
        }
        fadePanel.alpha = 0; // Ensure the panel is completely transparent at the end
        fadePanel.gameObject.SetActive(false); // Optionally deactivate the panel
    }

    // Coroutine to fade the screen out
    IEnumerator FadeScreenOut(string sceneName)
    {
        fadePanel.gameObject.SetActive(true); // Make sure the panel is active
        float duration = 1.0f;
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            fadePanel.alpha = currentTime / duration;
            yield return null;
        }
        fadePanel.alpha = 1; // Ensure the panel is completely opaque at the end

        // Load the scene after the fade-out
        SceneManager.LoadScene(sceneName);
    }


// This method sets the feedback message and starts the fade-in coroutine
public void ShowFeedbackMessage(string message)
{
    feedbackText.text = message;
    StartCoroutine(FadeCanvasGroup(TextCanvasGroup, 0f, 1f, 1f));
}

// Coroutine that fades a CanvasGroup's alpha value to a target value over a duration
private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float lerpTime = 1.0f)
{
    float _timeStartedLerping = Time.time;
    float timeSinceStarted = Time.time - _timeStartedLerping;
    float percentageComplete = timeSinceStarted / lerpTime;

    while (true)
    {
        timeSinceStarted = Time.time - _timeStartedLerping;
        percentageComplete = timeSinceStarted / lerpTime;

        float currentValue = Mathf.Lerp(start, end, percentageComplete);

        cg.alpha = currentValue;

        if (percentageComplete >= 1) break;

        yield return new WaitForEndOfFrame();
    }

    // When the fade-in completes, ensure the CanvasGroup is fully visible
    cg.alpha = end;
}




}
