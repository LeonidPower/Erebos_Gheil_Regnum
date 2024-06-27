using System.Collections;
using System; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Make sure this namespace is included
using TMPro;

public class Dialogue_Manager : MonoBehaviour
{
    // Public variables
    private Coroutine autoPlayCoroutine = null;
    private bool isFading = false;
    public GameObject TextBox_and_Butons;
    public TextMeshProUGUI dialogueText, speakerNameText;
    public Image characterImage;
    public Scene_Structure currentScene;
    public GameObject nextbackground;
    public AudioSource nextvoice, backgroundmusic;
    public float autoPlayDelay = 2.0f;
    public UI_Functionality UI_functionality;
    public AudioClip textBlipSound;
    public AudioSource textBlipAudioSource;

    public int sentenceIndex = -1;
    private bool isTyping = false, isAutoPlaying = false;
    private float TypeTextSpeed = 0.01f;
    private enum State { PLAYING, COMPLETED }
    private State state = State.COMPLETED;
    private Coroutine typingCoroutine;
    public CanvasGroup fadePanel;
    //private bool isLastScene = false;

    void Start()
    {
        UI_functionality.decisionFlag=-1;
        StartCoroutine(FadeScreenIn());
        PlayScene(currentScene);
       
    }
    void Update()
    {
        HandleInput();
    }
private void HandleInput()
{
    if(Input.GetKeyDown(KeyCode.G))
        {
             Debug.Log("g pressed, gallery");
            UI_functionality.Gallery();
          
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("d pressed, Dialogue log");
             UI_functionality.DialogueLog();
    }
    if (isFading || UI_functionality.decisionFlag == 0)
    {
        return; // If fading is in progress, ignore input
    }
    // Check if the Escape key is pressed to quit the game.
    if (Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Q))
    {
       
        UI_functionality.DecisionCloseGame();
        return; 
    }

    // // Check if any key is pressed while auto-play is active to stop it.
    // if (Input.anyKeyDown && isAutoPlaying)
    // {
    //     Debug.Log("HandleInput: Any key pressed while auto-playing. Stopping auto-play.");
    //     StopAutoPlay();
    //     return; // Return immediately to avoid further input processing.
    // }

    // Proceed with other checks only if background only state is not active.
    if (!UI_functionality.GetBackgroundOnlyState())
    {
        // Check for Space, Enter, or Right Arrow key for next sentence.
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("HandleInput: Space or Right Arrow key pressed.");
            HandleSpaceInput();
        }
         if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Shift + Enter pressed: Skipping to the next scene.");
            SkipToNextScene();
            return;
        }

        // Check for P, Backspace, or Left Arrow key for previous sentence or scene.
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("HandleInput: P, Backspace, or Left Arrow key pressed.");
            PlayPreviousSentenceOrScene();
        }

        // Check for A key to toggle auto-play.
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("HandleInput: A key pressed. Toggling auto-play.");
            ToggleAutoPlay();
        }
      
        if(Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("s pressed, Save");
            UI_functionality.SaveGame();
        }
          if(Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("l pressed, Load");
            UI_functionality.LoadGame();
        }
    }
}







    public void HandleSpaceInput()
{
   if(UI_functionality.GetBackgroundOnlyState())
   return ;

    if (isTyping)
    {
        CompleteSentence();
    }
    else if (IsCompleted())
    {
        if (isAutoPlaying)
        {
            ToggleAutoPlay(); // Toggle auto-play off if it's on and the scene is completed
        }
        else
        {
            PlayNextSentence();
        }
    }
}

    private void SkipToNextScene()
{
    if (currentScene.nextscene != null)
    {
        
        TextBox_and_Butons.SetActive(true); 
        StartCoroutine(FadeScreenOut(() => PlayScene(currentScene.nextscene)));
    }
    else
    {
        Debug.Log("No next scene to skip to.");

    }
}

    private void PlayNextSentence()
    {
        if (sentenceIndex < currentScene.sentences.Count - 1)
        {
            sentenceIndex++;
            StartCoroutine(UpdateSentenceDisplay());

        }
        else
            HandleSceneCompletion();
    }

private void HandleSceneCompletion()
{
    if (currentScene.nextscene != null)
    {
        // Start the fade out and then change the scene
        StartCoroutine(FadeScreenOut(() =>   PlayScene(currentScene.nextscene)));
    }
    else
    {
        // If there is no next scene, fade out and go to the credits
        StartCoroutine(FadeScreenOut(() => SceneManager.LoadScene("4Credits")));
    }
}



     public void PlayScene(Scene_Structure scene, int startSentenceIndex = 0)
{
        Debug.Log("PlayScene called. New Scene: " + (scene != null ? scene.name : "null"));
        currentScene = scene;
        sentenceIndex = startSentenceIndex - 1; // Set to one less than start index as PlayNextSentence increments it
        UpdateBackground(); // Separate background update from music update
        StartCoroutine(FadeScreenIn());
        PlayNextSentence();
        UpdateBackgroundMusic(); // Update music after starting the scene
}



    private void UpdateBackgroundAndMusic()
    {
        UpdateBackground();
        UpdateBackgroundMusic();
    }

public void PlayPreviousSentenceOrScene()
{
    if (sentenceIndex > 0)
    {
        // If there are sentences left in the current scene, go back one sentence.
        PlayPreviousSentence();
    }
    else if (currentScene != null && currentScene.previousscene != null && currentScene.previousscene.sentences.Count > 0)
    {
        // If it's the first sentence of the current scene and there is a previous scene
        StartCoroutine(ChangeSceneWithFade(currentScene.previousscene));
    }
}


private IEnumerator ChangeSceneWithFade(Scene_Structure nextScene)
{
    yield return StartCoroutine(FadeScreenOut()); // Fade out

    // Change the scene
    currentScene = nextScene;
    sentenceIndex = nextScene.sentences.Count - 1;
    UpdateBackgroundAndMusic();
    StartCoroutine(UpdateSentenceDisplay());

    PlayVoice();

    yield return StartCoroutine(FadeScreenIn()); // Fade in
}


    private void PlayPreviousSentence()
    {
        sentenceIndex--;
        StartCoroutine(UpdateSentenceDisplay());
    }
    public string getplayerName()
    {
        return PlayerPrefs.GetString("PlayerName", "Player");
    }

   private IEnumerator UpdateSentenceDisplay()
{
    var sentence = currentScene.sentences[sentenceIndex];
    // Replace "(name)" in the text with the player's name.
    string playerName = PlayerPrefs.GetString("PlayerName", "Player"); // Default to "Player" if no name is set
    sentence.text = sentence.text.Replace("(name)", playerName);

    if (sentence.speaker.speakerName == "(name)")
    {
        UI_functionality.Decision(sentence.text);
        // Wait until decisionFlag is set to 1
        yield return new WaitUntil(() => UI_functionality.decisionFlag == 1);
        // Use the decisionSentence directly
        sentence.text = UI_functionality.decisionSentence; // This assumes decisionSentence is of the correct type
        UI_functionality.decisionFlag = -1;
    }
    else
    {
        UI_functionality.HideDecisionBar();
    }

    speakerNameText.text = sentence.speaker.speakerName == "(name)" ? playerName : sentence.speaker.speakerName;
    speakerNameText.color = sentence.speaker.textColor;
    if (sentence.speaker.speakerName == "(name)")
    {
        Color playerColor = new Color(
            PlayerPrefs.GetFloat("PlayerColorR", 1f),
            PlayerPrefs.GetFloat("PlayerColorG", 1f),
            PlayerPrefs.GetFloat("PlayerColorB", 1f),
            PlayerPrefs.GetFloat("PlayerColorA", 1f)
        );
        speakerNameText.color = playerColor;
    }

    // If there is a characterSprite, set it. Otherwise, disable the GameObject that holds the Image component.
    if (sentence.speaker.characterSprite != null)
    {
        characterImage.sprite = sentence.speaker.characterSprite;
        characterImage.gameObject.SetActive(true);
        characterImage.color = new Color(1f, 1f, 1f, 1f); // Fully visible
    }
    else
    {
        characterImage.sprite = null;
        characterImage.gameObject.SetActive(false);
        characterImage.color = new Color(1f, 1f, 1f, 0f); // Fully transparent
    }

    PlayVoice();

    // Stop the existing typing coroutine before starting a new one
    if (typingCoroutine != null)
    {
        StopCoroutine(typingCoroutine);
    }
    typingCoroutine = StartCoroutine(TypeText(sentence.text));
}




    private IEnumerator TypeText(string text)
{
    dialogueText.text = "";
    state = State.PLAYING;
    isTyping = true;

    foreach (char c in text)
    {
        dialogueText.text += c;
        yield return new WaitForSeconds(TypeTextSpeed);
    }

    state = State.COMPLETED;
    isTyping = false;
    typingCoroutine = null; // Coroutine finished
}


 public void ToggleAutoPlay()
{
    if(UI_functionality.decisionFlag == 0)
    {
        return;
    }

    isAutoPlaying = !isAutoPlaying;
    Debug.Log($"ToggleAutoPlay: isAutoPlaying set to {isAutoPlaying}");

    if (isAutoPlaying)
    {
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine); // Ensure any existing coroutine is stopped
        }
        autoPlayCoroutine = StartCoroutine(AutoPlayDialogue()); // Start a new coroutine and keep a reference
        UI_functionality.SetStateBarText("Auto Play On", "Press A to turn off Auto play");
    }
    else
    {
        StopAutoPlay();
    }
}

public void StopAutoPlay()
{
    isAutoPlaying = false;
    if (autoPlayCoroutine != null)
    {
        StopCoroutine(autoPlayCoroutine);
        autoPlayCoroutine = null; // Clear the reference
    }
    Debug.Log("AutoPlay stopped.");
    // Update UI to reflect auto-play is off.
    UI_functionality.SetStateBarText("Auto Play Off", "Press A to turn on Auto play");
}


private IEnumerator AutoPlayDialogue()
{
    Debug.Log("AutoPlayDialogue: Coroutine started.");

    while (isAutoPlaying)
    {
        Debug.Log("AutoPlayDialogue: Waiting for typing to complete.");
        // Wait until the current sentence is fully displayed.
        yield return new WaitUntil(() => !isTyping);
        Debug.Log("AutoPlayDialogue: Typing completed.");

        // Wait for a decision to be made if decisionFlag is 0
        while (UI_functionality.decisionFlag == 0 && isAutoPlaying)
        {
            Debug.Log("AutoPlayDialogue: Waiting for decision to be made.");
            yield return null; // Wait indefinitely until the decisionFlag changes
        }
        yield return new WaitUntil(() => !UI_functionality.GetBackgroundOnlyState());
        Debug.Log("AutoPlayDialogue: Background only mode is off.");
        // After the sentence is displayed and any necessary decision is made, wait an additional delay.
        yield return new WaitForSeconds(autoPlayDelay);

        // Check if auto-play is still active before moving to the next sentence.
        if (!isAutoPlaying)
        {
            Debug.Log("AutoPlayDialogue: Auto-play has been stopped.");
            break;
        }

        if (IsLastSentence() && currentScene.nextscene != null)
        {
            // Wait for the fade-out before changing scenes
            yield return StartCoroutine(FadeScreenOut(() => PlayScene(currentScene.nextscene)));
        }
        else if (!IsLastSentence())
        {
            PlayNextSentence();
        }
    }

    Debug.Log("AutoPlayDialogue: Coroutine ended.");
}



public void StopAutoPlayAfterSentence()
{
    if (isTyping)
    {
        // Wait until the current sentence is completed before stopping auto-play
        StartCoroutine(WaitForSentenceCompletionBeforeStopping());
    }
    else
    {
        // If not currently typing, stop auto-play immediately
        StopAutoPlay();
    }
}
private IEnumerator WaitForSentenceCompletionBeforeStopping()
{
    yield return new WaitUntil(() => !isTyping);
    StopAutoPlay();
}
     

private void CompleteSentence()
{
    if (typingCoroutine != null)
    {
        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
    }

    // Fetch the player's name.
    string playerName = PlayerPrefs.GetString("PlayerName", "Player");

    // Check if the current sentence contains a '/', which is a decision point.
    if (currentScene.sentences[sentenceIndex].text.Contains("/"))
    {
        // The decisionSentence should already have the player's name replaced in it.
        dialogueText.text = UI_functionality.decisionSentence.Replace("(name)", playerName);
    }
    else
    {
        // Replace "(name)" with the actual player's name.
        string fullSentence = currentScene.sentences[sentenceIndex].text.Replace("(name)", playerName);
        // Display the updated sentence immediately.
        dialogueText.text = fullSentence;
    }

    state = State.COMPLETED;
    isTyping = false;
}




    private void UpdateBackground()
{
    dialogueText.text = "";
    speakerNameText.text = "";
    characterImage.sprite = null;
    characterImage.gameObject.SetActive(false);

    // Ensure there's an Image component to work with
    Image bgImage = nextbackground.GetComponent<Image>();
    if (bgImage == null)
    {
        Debug.LogError("Background image component is missing on the nextbackground GameObject.");
        return;
    }

    // If there's a background assigned in the current scene, use it
    if (currentScene.background != null)
    {
        bgImage.sprite = currentScene.background;
        TextBox_and_Butons.SetActive(currentScene.background.name != "NextDay");
    }
    else
    {
        // If no background is assigned, use a default white image or another placeholder
        bgImage.sprite = GetDefaultBackgroundSprite(); // Make sure to define this method
        TextBox_and_Butons.SetActive(true); // Set to true or false based on your game logic
    }
}
public void LoadGame()
{
    UI_functionality.LoadGame();
}

// This method should return a default Sprite (white or placeholder)
private Sprite GetDefaultBackgroundSprite()
{
    // Create a Texture2D with 1x1 pixel, set its color to white, and apply changes
    Texture2D defaultTexture = new Texture2D(1, 1);
    defaultTexture.SetPixel(0, 0, Color.white);
    defaultTexture.Apply();

    // Create a Sprite from the Texture2D
    return Sprite.Create(defaultTexture, new Rect(0, 0, defaultTexture.width, defaultTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
}

    public void UpdateBackgroundMusic()
    {
        Debug.Log("UpdateBackgroundMusic called. Current Scene: " + (currentScene != null ? currentScene.name : "null"));
    if (currentScene != null && currentScene.BackgroundMusic != null)
    {
        Debug.Log("Background music clip found. Playing music: " + currentScene.BackgroundMusic.name);
        backgroundmusic.clip = currentScene.BackgroundMusic;
        backgroundmusic.Play();
    }
    else
    { /// HERE SOS ITS FOR GALLLERY MUSIC STOP/PLAY DEFULAT AND LOAD MUSIC NOT WORKING 
        Debug.LogWarning("No background music clip found for the current scene. Stopping music.");
        backgroundmusic.Stop();
    }
    }
    public AudioClip  GetDefaultSceneMusicClip()
    {
        return currentScene.BackgroundMusic;
    }
    public void PlayVoice()
    {
        nextvoice.Stop();
        if (currentScene.sentences[sentenceIndex].voice != null)
            nextvoice.PlayOneShot(currentScene.sentences[sentenceIndex].voice);
    }

    public void SetTextSpeed(float speed)
    {
        // Adjust the speed of text typing
        TypeTextSpeed = speed;
    }

    public bool IsCompleted()
    {
        // Check if the state is completed
        return state == State.COMPLETED;
    }

    public bool IsLastSentence()
    {
        // Check if the current sentence is the last in the scene
        return sentenceIndex >= currentScene.sentences.Count - 1;
    }
    public bool IsTyping { get { return isTyping; } }
    public bool IsAutoPlaying { get { return isAutoPlaying; } }

      IEnumerator FadeScreenIn()
    {
          isFading = true;
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
         isFading = false;
    }
    

    // Coroutine to fade the screen out
 private IEnumerator FadeScreenOut(Action onComplete = null)
{
    isFading = true;
    Debug.Log("FadeScreenOut: Started");
    fadePanel.gameObject.SetActive(true);
    float duration = 1.0f;
    float currentTime = 0f;
    while (currentTime < duration)
    {
        currentTime += Time.deltaTime;
        fadePanel.alpha = currentTime / duration;
        yield return null;
    }
    fadePanel.alpha = 1; // Ensure the panel is completely opaque at the end
    Debug.Log("FadeScreenOut: Completed");
    isFading = false;
    onComplete?.Invoke(); // Call the onComplete action if it's not null
}


  public string GetCurrentSpeaker()
    {
       return currentScene.sentences[sentenceIndex].speaker.speakerName;
    }

    public string GetCurrentSentence()
    {
        return currentScene.sentences[sentenceIndex].text;
    }
    

}