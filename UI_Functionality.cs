using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic; // For Dictionary
using System;
using Unity.VisualScripting;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.SceneManagement;
public class UI_Functionality : MonoBehaviour
{
    public Button  DialogueWriteButton,DialogueLogButton,GallerySoundEffectsTabButton,GalleryMusicTabButton, SButton, LButton, BButton, MButton, CButton, CloseConfig, NextButton, PrevButton,GalleryButton,CloseGameButton,D1Button,D2Button,CloseGalleryButton,BackgroundsTabButton, CharactersTabButton,MusicTabButton, SoundEffectsTabButton,GallerySelectButton,retryButton;
    public GameObject textBoxAndButtons, SpeakerImage, StateBar, DecisionBar, ConfigurationBar,GalleryBar,DialogueLogBar;
    public TextMeshProUGUI StateTitle, StateText, DecisionTitle, D1Text, D2Text,GallerySelectButtonText ;
    public CanvasGroup stateBarCanvasGroup;
    public TMP_Dropdown screenModeDropdown;
    public Slider SizeSlider,textSpeedSlider, musicVolumeSlider, voiceVolumeSlider;
    public AudioSource musicAudioSource, voiceAudioSource,SFXAudioSource; 
    public Dialogue_Manager dialogueManager;
    public GameObject fullScreenPanel;
    public Image fullScreenImage;
    public GameObject BlockScreen;
    private bool configIsActive = false, isMuted = false, isProcessing = false, backgroundOnly = false,galleryIsActive=false;
    public GameObject AudioPrefab,imagePrefab,TextPrefab;
    public Transform imagegalleryContent,audiogalleryContent; 
    public TextMeshProUGUI DialogueLogText;
    public GameObject ImageScrollView,AudioScrollView;
    public ScrollRect TextScrollView;
    private int tempWidth,tempHeight;
    private Coroutine fadeStateBarCoroutine;
    public int decisionFlag=-1;
    private bool isFirstGalleryOpen = true;
    public string decisionSentence;
   
    void Start()
    {

        InitializeUI();
        AssignButtonListeners();
        ApplySettings();
    }

    void Update()
    {
        HandleKeyboardInputs();
       
    }

    private void InitializeUI()
    {
        // Set initial values for UI elements
        textSpeedSlider.value = 0.5f;
        musicVolumeSlider.value = 0.5f;
        voiceVolumeSlider.value = 0.5f;
        StateBar.SetActive(false);
        DecisionBar.SetActive(false);
        ConfigurationBar.SetActive(false);
        GalleryBar.SetActive(false);
        GallerySelectButtonText.text = "Art";
        GalleryMusicTabButton.gameObject.SetActive(false);
        GallerySoundEffectsTabButton.gameObject.SetActive(false);
        fullScreenPanel.SetActive(false);
        DialogueLogBar.SetActive(false);
        SizeSlider.gameObject.SetActive(false);
        BlockScreen.SetActive(false);
        retryButton.gameObject.SetActive(false);
    }
    private void ApplySettings()
    {
        // Apply text speed
        float textSpeed = PlayerPrefs.GetFloat("TextSpeed", 0.5f); // Default to 0.5 if not set
        textSpeedSlider.value = textSpeed;
        SetTextSpeed(textSpeed);
        Debug.Log("Text Speed Loaded: " + textSpeed);

        // Apply music volume
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        musicVolumeSlider.value = musicVolume;
        musicAudioSource.volume = musicVolume;
        Debug.Log("Music Volume Loaded: " + musicVolume);

        // Apply voice volume
        float voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 0.5f);
        voiceVolumeSlider.value = voiceVolume;
        voiceAudioSource.volume = voiceVolume;
        Debug.Log("Voice Volume Loaded: " + voiceVolume);

        // Apply screen mode
        int screenMode = PlayerPrefs.GetInt("ScreenMode", 0);
        screenModeDropdown.value = screenMode;
        OnScreenModeChange(screenMode);
        Debug.Log("Screen Mode Loaded: " + screenMode);

       
        tempWidth = PlayerPrefs.GetInt("WindowWidth");
        tempHeight = PlayerPrefs.GetInt("WindowHeight");
        ApplySizeChange();
        Debug.Log("slider size:"+PlayerPrefs.GetFloat("WindowSize"));
        SizeSlider.value = PlayerPrefs.GetFloat("WindowSize"); 

    }
public void ShowFullScreenImage(Sprite imageSprite)
{
    fullScreenImage.sprite = imageSprite;
    fullScreenImage.color = new Color(1f, 1f, 1f, 1f); // Ensure image is fully visible

    // Assuming 'Preserve Aspect' is enabled on the Image component,
    // and the Image is set to fill the parent container (FullScreenPanel).
    fullScreenImage.preserveAspect = true;

    // Optionally, adjust the FullScreenPanel to be the size of the screen or use anchors to fill the screen.
    RectTransform panelRT = fullScreenPanel.GetComponent<RectTransform>();
    panelRT.sizeDelta = new Vector2(Screen.width, Screen.height); // Adjust if necessary based on your UI setup
    
    fullScreenPanel.SetActive(true);
    
    // Debug log to check the actual size after adjustments
    Debug.Log($"Image displayed with Preserve Aspect. Panel size: {Screen.width} x {Screen.height}");
}


public void HideFullScreenImage()
{
    fullScreenPanel.SetActive(false);
}
    private void AssignButtonListeners()
    {
        // Assign listeners to buttons
        SButton.onClick.AddListener(SaveGame);
        LButton.onClick.AddListener(LoadGame);
        BButton.onClick.AddListener(BackgroundOnly);
        MButton.onClick.AddListener(Mute);
        CButton.onClick.AddListener(Configuration);
        CloseConfig.onClick.AddListener(CloseConfiguration);
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChange);
        textSpeedSlider.onValueChanged.AddListener(delegate { SetTextSpeed(textSpeedSlider.value); });
        musicVolumeSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicVolumeSlider.value); });
        voiceVolumeSlider.onValueChanged.AddListener(delegate { SetVoiceVolume(voiceVolumeSlider.value); });
        SizeSlider.onValueChanged.AddListener(OnSizeChanged);
        AddPointerUpListener(SizeSlider, ApplySizeChange);
        GalleryButton.onClick.AddListener(Gallery);
        CloseGalleryButton.onClick.AddListener(CloseGallery);
        GallerySelectButton.onClick.AddListener(GallerySelect);
        BackgroundsTabButton.onClick.AddListener(ShowBackgrounds);
        CharactersTabButton.onClick.AddListener(ShowCharacters);
        MusicTabButton.onClick.AddListener(ShowMusic);
        SoundEffectsTabButton.onClick.AddListener(ShowSoundEffects);
        CloseGameButton.onClick.AddListener(DecisionCloseGame);
        NextButton.onClick.AddListener(() => {
            if (decisionFlag != 0) {
                dialogueManager.HandleSpaceInput();
            }
        });
        PrevButton.onClick.AddListener(() => {
            if (decisionFlag != 0) {
                dialogueManager.PlayPreviousSentenceOrScene();
            }
        });
        DialogueLogButton.onClick.AddListener(DialogueLog);
    }

    private void HandleKeyboardInputs()
    {
        // Handle keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.B))
            BackgroundOnly();
        if (Input.GetKeyDown(KeyCode.M))
            Mute();
        if (Input.GetKeyDown(KeyCode.C))
            Configuration();
        if(Input.GetKeyDown(KeyCode.W))
            WriteToLog();

    }
[Serializable]
public class SaveData
{
    public string currentSceneName;
    public int sentenceIndex;
    public List<string> dialogueLogEntries = new List<string>(); // Added line for dialogue log entries
    public float textSpeed;
    public float musicVolume;
    public float voiceVolume;
    public int screenMode; // Add this line
    public float windowSize;
    public int windowWidth;
    public int windowHeight;
}
public void SaveGame()
{
    if(decisionFlag==0)
    return;

    if (dialogueManager.currentScene != null && !string.IsNullOrEmpty(dialogueManager.currentScene.name))
    {
         SaveData saveData = new SaveData
        {
            currentSceneName = dialogueManager.currentScene.name,
            sentenceIndex = dialogueManager.sentenceIndex,
            dialogueLogEntries = GetDialogueLogEntries(), // Call a method to get current log entries
            textSpeed = textSpeedSlider.value, // Save text speed setting
            musicVolume = musicVolumeSlider.value, // Save music volume setting
            voiceVolume = voiceVolumeSlider.value, // Save voice volume setting
            screenMode = screenModeDropdown.value, // Save screen mode setting
            windowSize = SizeSlider.value, // Save window size slider value
            windowWidth = tempWidth,
            windowHeight = tempHeight
        };

        string json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/savegame.json", json);
        Debug.Log("Game Saved. Path: " + Application.persistentDataPath + "/savegame.json");
        Debug.Log("Saved Scene Name: " + saveData.currentSceneName);
        // Update the state bar to show "Game Saved" message
        SetStateBarText("Game Saved", "Your game has been successfully saved");
    }
    else
    {
        Debug.LogError("Error: The current scene is null or the name is empty.");
    }
}


private List<string> GetDialogueLogEntries()
{
    List<string> entries = new List<string>();
    foreach (Transform entry in TextScrollView.content)
    {
        TextMeshProUGUI textComponent = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            entries.Add(textComponent.text);
        }
    }
    return entries;
}

public void LoadGame()
{
    if (decisionFlag == 0)
        return;

    string path = Application.persistentDataPath + "/savegame.json";

    if (System.IO.File.Exists(path))
    {
        string json = System.IO.File.ReadAllText(path);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("Loading scene: " + saveData.currentSceneName);

        Scene_Structure loadedScene = Resources.Load<Scene_Structure>("2Dialogue_Scenes/" + saveData.currentSceneName.Trim());

        textSpeedSlider.value = saveData.textSpeed;
        musicVolumeSlider.value = saveData.musicVolume;
        voiceVolumeSlider.value = saveData.voiceVolume;
        SizeSlider.value = saveData.windowSize;
        tempWidth = saveData.windowWidth;
        tempHeight = saveData.windowHeight;

        SetTextSpeed(saveData.textSpeed);
        SetMusicVolume(saveData.musicVolume);
        SetVoiceVolume(saveData.voiceVolume);
        ApplySizeChange(); // Apply window size change
        // Clear existing dialogue log entries before loading new ones
        ClearDialogueLog();

        if (saveData.dialogueLogEntries != null && saveData.dialogueLogEntries.Count > 0)
        {
            foreach (string logEntry in saveData.dialogueLogEntries)
            {
                CreateDialogueLogEntry(logEntry); // Recreate log entries from the saved data
            }
        }

        if (loadedScene != null)
        {
            dialogueManager.PlayScene(loadedScene, saveData.sentenceIndex);
            SetStateBarText("Game Loaded", "Your game has been successfully loaded");

            // Ensure background music is updated after loading the scene
            Debug.Log("Calling UpdateBackgroundMusic to play the background music.");
            dialogueManager.UpdateBackgroundMusic();
        }
        else
        {
            Debug.LogError("Saved scene not found.");
        }
    }
    else
    {
        Debug.LogError("No save file found.");
    }
}





private void ClearDialogueLog()
{
    // Clear the existing log entries
    foreach (Transform child in TextScrollView.content)
    {
        Destroy(child.gameObject);
    }
}

private void CreateDialogueLogEntry(string text)
{
    GameObject entry = Instantiate(TextPrefab, TextScrollView.content.transform);
    TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
    entryText.text = text;

    // Find the delete button in the instantiated prefab and add the listener
    Button deleteButton = entry.GetComponentInChildren<Button>();
    if (deleteButton != null)
    {
        // Remove all listeners to ensure no duplicates
        deleteButton.onClick.RemoveAllListeners();
        // Add the listener to call DeleteEntry when the button is clicked
        deleteButton.onClick.AddListener(() => DeleteEntry(entry));
    }

    entry.SetActive(true);
}

      public void BackgroundOnly()
    {

         if (decisionFlag == 0)
    {
        Debug.Log("Decision in progress, cannot enter background only mode.");
        return;
    }
        
        if (!isProcessing)
        {
            isProcessing=true;
            bool isActive = textBoxAndButtons.activeSelf;
            backgroundOnly = !backgroundOnly;
             if (backgroundOnly && dialogueManager.IsAutoPlaying)
            {
                dialogueManager.StopAutoPlay();
            }
            SpeakerImage.SetActive(!isActive);
            textBoxAndButtons.SetActive(!isActive);
            if(isActive){
           
            StateTitle.text = "Background Only";
            StateText.text = "Press B to show user interface";

       
                StateBar.SetActive(true);
                StartCoroutine(FadeStateBar());
            }
            else
               isProcessing=!isProcessing; 
            }
    }

  

private void UpdateUIVisibility()
{
    SpeakerImage.SetActive(!backgroundOnly);
    textBoxAndButtons.SetActive(!backgroundOnly);
}

private void UpdateStateBar()
{
    StateTitle.text = backgroundOnly ? "Background Only" : "Normal Mode";
    StateText.text = backgroundOnly ? "Press B for UI" : "Press B for Background Only";
    StateBar.SetActive(true);
    StartCoroutine(FadeStateBar());
}


public void Mute()
{
    if (StateTitle.text == "You Died")
    {
        Debug.Log("Mute action disabled in 'You Died' state.");
        return;
    }

    if (!isProcessing)
    {
        isProcessing = true;
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0 : 1;
        StateTitle.text = isMuted ? "Audio Off" : "Audio On";
        StateText.text = isMuted ? "Press M to unmute" : "Audio unmuted";
        StateBar.SetActive(true);
        StartCoroutine(FadeStateBar());
    }
}

public void Configuration()
{
    configIsActive = !configIsActive;
    ConfigurationBar.SetActive(configIsActive);
    
}

public void CloseConfiguration()
{
    ConfigurationBar.SetActive(false);
    configIsActive = false;
}

public void SetTextSpeed(float speed)
{
    float mappedSpeed = Mathf.Lerp(0.1f, 0.01f, speed);
    if (dialogueManager != null)
    {
        dialogueManager.SetTextSpeed(mappedSpeed);
    }
}

public void SetMusicVolume(float volume)
{
    musicAudioSource.volume = volume;
}

public void SetVoiceVolume(float volume)
{
    voiceAudioSource.volume = volume;
}

public void OnScreenModeChange(int modeIndex)
{
    PlayerPrefs.SetInt("ScreenMode", modeIndex); // Save the screen mode setting
    PlayerPrefs.Save();
     switch (modeIndex)
    {
        case 0: // Fullscreen
            SetFullScreen();
            HideResolutionSliders();
            break;
        case 1: // Windowed
            SetWindowedMode();
            ShowResolutionSliders();
            break;
    }
}

public void SetFullScreen()
{
    Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    HideResolutionSliders(); 
}

  private void SetWindowedMode()
{
    ShowResolutionSliders();
    SizeSlider.maxValue = 1.0f;
    SizeSlider.minValue = 0.2f;
    tempWidth = Screen.currentResolution.width / 2;
    tempHeight = Screen.currentResolution.height / 2;
    Screen.SetResolution(tempWidth, tempHeight, false);
    UpdateSlider();
}


private void UpdateSlider()
{
    // Update the slider to reflect the current window size as a percentage of the full screen size
    float currentValue = (float)Screen.width / Screen.currentResolution.width;
    SizeSlider.value = currentValue;
}
private void ShowResolutionSliders()
{
    SizeSlider.gameObject.SetActive(true);
    SizeSlider.value = 0.5f;
}

public void OnSizeChanged(float value)
{
    float sizePercentage = Mathf.Lerp(0.35f, 0.85f, Mathf.InverseLerp(SizeSlider.minValue, SizeSlider.maxValue, value));
    tempWidth = Mathf.RoundToInt(Screen.currentResolution.width * sizePercentage);
    float aspectRatio = (float)Screen.currentResolution.width / Screen.currentResolution.height;
    tempHeight = Mathf.RoundToInt(tempWidth / aspectRatio);

    
}

private void HideResolutionSliders()
{
    SizeSlider.gameObject.SetActive(false);
}

 public void ApplySizeChange()
    {
        if (!Screen.fullScreen)
        {
            Debug.Log($"Applying size change: {tempWidth}x{tempHeight}");
            Screen.SetResolution(tempWidth, tempHeight, false);

        }
    }



public void SetStateBarText(string title, string text)
{
     if (fadeStateBarCoroutine != null)
    {
        StopCoroutine(fadeStateBarCoroutine);
    }
    StateTitle.text = title;
    StateText.text = text;
    StateBar.SetActive(true);
    fadeStateBarCoroutine = StartCoroutine(FadeStateBar());
}

private void AddPointerUpListener(Slider slider, UnityEngine.Events.UnityAction call)
{
    // First remove all existing PointerUp triggers to avoid duplicates
    EventTrigger trigger = slider.GetComponent<EventTrigger>() ?? slider.gameObject.AddComponent<EventTrigger>();
    trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.PointerUp);

    // Create a new entry for the PointerUp event
    EventTrigger.Entry entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerUp;
    entry.callback.AddListener((data) => { call(); });

    // Add the entry to the triggers list
    trigger.triggers.Add(entry);
}


public bool GetBackgroundOnlyState()
{
    return backgroundOnly;
}


public void Decision(string sentenceWithOptions)
{
     BlockScreen.SetActive(true);
    // Split the sentence into two options based on '/'
    string[] options = sentenceWithOptions.Split(new char[] { '/' }, 2);

    if(options.Length == 2)
    {
         decisionFlag = 0;
        // There are two options
        bool decisionIsActive = DecisionBar.activeSelf;
        if(!decisionIsActive)
        {
          
            DecisionBar.SetActive(true);
            DecisionTitle.text = "Decide";
            
           
            D1Text.text = options[0];
            D2Text.text = options[1];

            // Add listeners to the buttons that set the chosen option and flag
            D1Button.onClick.RemoveAllListeners();
            D1Button.onClick.AddListener(() => 
            {
                 if (options[0].Contains("*Block*"))
                {
                    Debug.Log("You chose to block the attack!");
                    DeathChosen("How did you expect to block that? You got struck by a sword and died...");
                }
                else
                 if (options[0].Contains("Feel free to take a bite!"))
                {
                    Debug.Log("Feel free to take a bite!");
                    DeathChosen("Why would you let a crazy stranger bat girl take your blood? Of course she is going to eat you uncontrollably!");
                }
                else{
                decisionSentence = options[0];
                decisionFlag = 1;
                DecisionBar.SetActive(false);
                BlockScreen.SetActive(false);
                }

                
               

                
            });

            D2Button.onClick.RemoveAllListeners();
            D2Button.onClick.AddListener(() => 
            {
                if (options[1].Contains("Nah, you are creepy!"))
                {
                    Debug.Log("Nah, you are creepy!");
                    DeathChosen("You told a crazy girl she is creepy, she will hunt you down for it");
                }
                 else{
                decisionSentence = options[1];
                decisionFlag = 1;
                DecisionBar.SetActive(false);
                BlockScreen.SetActive(false);
                 }
            });
        }
        else
        {
            DecisionBar.SetActive(false);
        }
    }
    else
    {
        // Handle error: the sentence does not contain two options
        Debug.LogError("The sentence does not contain two options separated by '/'");
    }
    
}

public void DecisionCloseGame()
{
    decisionFlag = 0;
    BlockScreen.SetActive(true);
    bool decisionisactive = DecisionBar.activeSelf;
    if(!decisionisactive){
    DecisionBar.SetActive(true);
    D1Text.text = "Yes";
    D2Text.text = "No";
    DecisionTitle.text = "Are you sure you want to quit?";
    D1Button.onClick.AddListener(() => QuitGame());
   D2Button.onClick.AddListener(() => {
    DecisionBar.SetActive(false);
    decisionFlag = 1;
    BlockScreen.SetActive(false);
});

    }
    else
     DecisionBar.SetActive(false);
}

public void Gallery()
{
   
      // Toggle the active state of the gallery
    galleryIsActive = !galleryIsActive;
    
    if (isFirstGalleryOpen)
    {
        // On first opening, ensure the gallery is active and show characters
        GalleryBar.SetActive(true); // Ensure the gallery is active
        ShowCharacters(); // Open the Characters tab by default
        
        isFirstGalleryOpen = false; // Mark that the gallery has now been opened once
    }
    else
    {
        // For subsequent openings, just toggle the visibility without altering the tab
        GalleryBar.SetActive(galleryIsActive);
    }
}
    public void GallerySelect()
{
    if (GallerySelectButtonText.text == "Art")
    {
        // Switch to Music
        GallerySelectButtonText.text = "Music";
        SoundEffectsTabButton.gameObject.SetActive(true);
        MusicTabButton.gameObject.SetActive(true);
        BackgroundsTabButton.gameObject.SetActive(false);
        CharactersTabButton.gameObject.SetActive(false);

        // Disable image gallery and enable audio gallery
        imagegalleryContent.gameObject.SetActive(false);
        ImageScrollView.SetActive(false);
        audiogalleryContent.gameObject.SetActive(true);
        AudioScrollView.SetActive(true);
        ShowMusic(); // Call this to populate the music gallery if it's not already populated
    }
    else if (GallerySelectButtonText.text == "Music")
    {
        // Switch to Art
        GallerySelectButtonText.text = "Art";
        BackgroundsTabButton.gameObject.SetActive(true);
        CharactersTabButton.gameObject.SetActive(true);
        SoundEffectsTabButton.gameObject.SetActive(false);
        MusicTabButton.gameObject.SetActive(false);

        // Disable audio gallery and enable image gallery
        audiogalleryContent.gameObject.SetActive(false);
        AudioScrollView.SetActive(false);
        imagegalleryContent.gameObject.SetActive(true);
        ImageScrollView.SetActive(true);
        ShowCharacters(); // Call this to refresh the backgrounds gallery if needed
    }
}

     public void CloseGallery()
    {
      GalleryBar.SetActive(false);
      galleryIsActive = false;
    
      
    }
    private void ShowBackgrounds()
{
   
     // Clear existing images in gallery
        foreach (Transform child in imagegalleryContent)
        {
            Destroy(child.gameObject);
        }

        // Load all textures from Resources/ArtWork/Backgrounds
        Texture2D[] textures = Resources.LoadAll<Texture2D>("ArtWork/Backgrounds");
         foreach (var texture in textures)
    {
        GameObject imageObj = Instantiate(imagePrefab, imagegalleryContent);
        Image image = imageObj.GetComponent<Image>();
        if (image != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            image.sprite = sprite;

            // Add an event listener to the Image's Button component to call ShowFullScreenImage
            image.GetComponent<Button>().onClick.AddListener(() => ShowFullScreenImage(sprite));
        }
    }
     GridLayoutGroup gridLayoutGroup = imagegalleryContent.GetComponent<GridLayoutGroup>();
    if (gridLayoutGroup != null)
    {
        // Set the Y cell size for backgrounds
        gridLayoutGroup.cellSize = new Vector2(gridLayoutGroup.cellSize.x, 135);
    }
      ArtResetScrollViewPosition(ImageScrollView);
}

private void ShowCharacters()
{
    
      // Clear existing images in gallery
    foreach (Transform child in imagegalleryContent)
    {
        Destroy(child.gameObject);
    }

    // Load all textures from Resources/ArtWork/Characters
    Texture2D[] textures = Resources.LoadAll<Texture2D>("ArtWork/Characters");
    foreach (var texture in textures)
    {
        // Create a new UI Image for each texture
        GameObject imageObj = Instantiate(imagePrefab, imagegalleryContent);
        Image image = imageObj.GetComponent<Image>();
        if (image != null)
        {
            // Convert Texture2D to Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            image.sprite = sprite;

            // Adjust the size and scale of the image
            image.rectTransform.sizeDelta = new Vector2(200, 200); // Set this to your desired size

            // Add an event listener to the Image's Button component to call ShowFullScreenImage
            image.GetComponent<Button>().onClick.AddListener(() => ShowFullScreenImage(sprite));
        }
    }
     GridLayoutGroup gridLayoutGroup = imagegalleryContent.GetComponent<GridLayoutGroup>();
    if (gridLayoutGroup != null)
    {
        // Set the Y cell size for characters
        gridLayoutGroup.cellSize = new Vector2(gridLayoutGroup.cellSize.x, 240);
    }
    ArtResetScrollViewPosition(ImageScrollView);
}
private void ArtResetScrollViewPosition(GameObject scrollView)
{
    Canvas.ForceUpdateCanvases(); // Force all pending layout changes to be applied immediately
    ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
    if (scrollRect != null)
    {
        scrollRect.verticalNormalizedPosition = 1f; // 1f for the top, 0f for the bottom
        Canvas.ForceUpdateCanvases(); // Force a second update after setting the position
    }
}

private void MusicResetScrollViewPosition()
{
    Canvas.ForceUpdateCanvases(); // Force all pending layout changes to be applied immediately
    // Assuming AudioScrollView is the GameObject containing the ScrollRect for your music gallery
    if (AudioScrollView != null)
    {
        ScrollRect scrollRect = AudioScrollView.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            // Set the scrollbar to the top position
            scrollRect.verticalNormalizedPosition = 1f; // 1f for the top, 0f for the bottom
            Canvas.ForceUpdateCanvases(); // Force a second update after setting the position
        }
    }
}

public void ShowMusic()
{
     
    // Clear existing items in audio gallery
    foreach (Transform child in audiogalleryContent)
    {
        Destroy(child.gameObject);
    }

    // Load all music clips from Resources/Audio/Music
    AudioClip[] musicClips = Resources.LoadAll<AudioClip>("Audio/Music");
    foreach (var clip in musicClips)
    {
        // Instantiate the music item prefab
        GameObject musicItemObj = Instantiate(AudioPrefab, audiogalleryContent);

        // Set the name
        TextMeshProUGUI musicName = musicItemObj.transform.Find("AudioName")?.GetComponent<TextMeshProUGUI>();
        if (musicName != null) musicName.text = clip.name;

        // Set up the play button
        Button playButton = musicItemObj.transform.Find("AudioPlayButton")?.GetComponent<Button>();
        if (playButton != null) playButton.onClick.AddListener(() => PlayMusic(clip, musicItemObj));

        // Set up the progression slider
        Slider progressionSlider = musicItemObj.transform.Find("AudioSlider")?.GetComponent<Slider>();
        if (progressionSlider != null)
        {
            progressionSlider.maxValue = clip.length;
            progressionSlider.value = 0;
            progressionSlider.interactable = false; // Make it non-interactable if it's only for display
        }
    }
  MusicResetScrollViewPosition();
}





private void PlayMusic(AudioClip clip, GameObject musicItemObj)
{
    // Check if the audio source is already playing the requested clip
    if (musicAudioSource.isPlaying && musicAudioSource.clip == clip)
    {
        // Stop the current clip and revert to the scene's default music if necessary
        musicAudioSource.Stop();
        // Optionally, play the default scene music here if you have one
        PlayDefaultSceneMusic();
    }
    else
    {
        // Play the selected clip
        musicAudioSource.clip = clip;
        musicAudioSource.Play();
        StartCoroutine(UpdateMusicSlider(musicItemObj, clip));
    }
}

private void PlayDefaultSceneMusic()
{
    // Set the default music clip for the scene
    // Assuming you have a method to get the default music clip for the current scene
    AudioClip defaultMusic = dialogueManager.GetDefaultSceneMusicClip(); // Implement this method based on your project
    if (defaultMusic != null)
    {
        musicAudioSource.clip = defaultMusic;
        musicAudioSource.Play();
    }
}


private IEnumerator UpdateMusicSlider(GameObject musicItemObj, AudioClip clip)
{
    Slider progressionSlider = musicItemObj.transform.Find("AudioSlider").GetComponent<Slider>();
    while (musicAudioSource.isPlaying && musicAudioSource.clip == clip)
    {
        progressionSlider.value = musicAudioSource.time;
        yield return null;
    }
    progressionSlider.value = 0; // Reset slider when done
}

public void ShowSoundEffects()
{
    
    // Clear existing items in sound effects gallery
    foreach (Transform child in audiogalleryContent)
    {
        Destroy(child.gameObject);
    }

    // Load all sound effect clips from Resources/Audio/SoundEffects
    AudioClip[] soundEffectClips = Resources.LoadAll<AudioClip>("Audio/SoundEffects");
    Debug.Log("Loading sound effects...");
    foreach (var clip in soundEffectClips)
    {
        Debug.Log("Loaded sound effect: " + clip.name);
        // Instantiate the audio item prefab
        GameObject soundEffectItemObj = Instantiate(AudioPrefab, audiogalleryContent);

        // Set the name
        TextMeshProUGUI soundEffectName = soundEffectItemObj.transform.Find("AudioName")?.GetComponent<TextMeshProUGUI>();
        if (soundEffectName != null) soundEffectName.text = clip.name;

        // Set up the play button
        Button playButton = soundEffectItemObj.transform.Find("AudioPlayButton")?.GetComponent<Button>();
        if (playButton != null) playButton.onClick.AddListener(() => PlaySoundEffect(clip, soundEffectItemObj));

        // Setup the progression slider (if needed)
        Slider progressionSlider = soundEffectItemObj.transform.Find("AudioSlider")?.GetComponent<Slider>();
        if (progressionSlider != null)
        {
            progressionSlider.maxValue = clip.length;
            progressionSlider.value = 0;
            // Set slider to not interactable if it is only for display purposes
            progressionSlider.interactable = false;
        }
    }
     MusicResetScrollViewPosition();
}

public void HideDecisionBar()
{
    DecisionBar.SetActive(false);
}
 public void DialogueLog()
{
    // Toggle the active state of the dialogue log
    DialogueLogBar.SetActive(!DialogueLogBar.activeSelf);
 
}

 public void WriteToLog()
{
    // Create new dialogue entry
       if (decisionFlag == 0)
    {
        Debug.Log("Cannot write to log while a decision is pending.");
        return;
    }

    GameObject entry = Instantiate(TextPrefab, TextScrollView.content.transform); // Make sure it's the 'content' transform

    // Set the text for the dialogue entry
    TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
    string currentSpeaker = dialogueManager.GetCurrentSpeaker();
    string currentSentence = dialogueManager.GetCurrentSentence();
    if(currentSpeaker!="(name)")
    entryText.text = $"{currentSpeaker}: {currentSentence}";
    else
    entryText.text = $"{dialogueManager.getplayerName()}: {decisionSentence}";

    // Find the delete button and add a listener to its onClick event
    Button deleteButton = entry.GetComponentInChildren<Button>();
    deleteButton.onClick.AddListener(() => DeleteEntry(entry));

    // Ensure the prefab is active and visible
    entry.SetActive(true);

    // Scroll to the bottom to show the latest entry
    Canvas.ForceUpdateCanvases();
    TextScrollView.verticalNormalizedPosition = 0f; // 0f for bottom, 1f for top
}

    private void DeleteEntry(GameObject entry)
    {
        Destroy(entry);
    }
private void PlaySoundEffect(AudioClip clip, GameObject soundEffectItemObj)
{
    SFXAudioSource.clip = clip;
    SFXAudioSource.Play();
        StartCoroutine(UpdateSFXSlider(soundEffectItemObj, clip));
}

private IEnumerator UpdateSFXSlider(GameObject soundEffectItemObj, AudioClip clip)
{
    Slider progressionSlider = soundEffectItemObj.transform.Find("AudioSlider").GetComponent<Slider>();
    while (SFXAudioSource.isPlaying && SFXAudioSource.clip == clip)
    {
        progressionSlider.value = SFXAudioSource.time;
        yield return null;
    }
    progressionSlider.value = 0; 
}

public void DeathChosen(string deathReason)
{
    DecisionBar.SetActive(false);
    decisionFlag=0;
    BlockScreen.SetActive(true);
    Debug.Log("DeathChosen called with reason: " + deathReason); // Debug statement
    StateTitle.text = "You Died";
    StateText.text = deathReason;
    stateBarCanvasGroup.alpha = 0; // Ensure this is set to 0
    stateBarCanvasGroup.blocksRaycasts = true; // Ensure the bar can block raycasts
    StateBar.SetActive(true); // Activate the StateBar
    retryButton.gameObject.SetActive(true);
    retryButton.interactable = true;
    StartCoroutine(FadeCanvasGroup(stateBarCanvasGroup, 0f, 1f, 0.5f)); // Start the coroutine
}


public void RetryButton()
{
    // Load the scene from the first sentence
    
    decisionFlag=1;
    // Hide the retry button
    retryButton.gameObject.SetActive(false);
    retryButton.interactable = false;
    CanvasGroup retryButtonCanvasGroup = retryButton.GetComponent<CanvasGroup>();
    if (retryButtonCanvasGroup != null)
    {
        retryButtonCanvasGroup.alpha = 0; // Ensure button is fully invisible
    }

    // Deactivate the state bar
    StateBar.SetActive(false);

    // Unblock the screen
    BlockScreen.SetActive(false);

    // Optionally, stop any running coroutines that might be fading or updating the state bar
    if (fadeStateBarCoroutine != null)
    {
        StopCoroutine(fadeStateBarCoroutine);
    }
      
         StartCoroutine(DelayedSceneReload(0.005f));
      
}
private IEnumerator DelayedSceneReload(float delay)
{
    // Wait for the specified delay
    yield return new WaitForSeconds(delay);

    if (dialogueManager.currentScene != null)
    {
        dialogueManager.PlayScene(dialogueManager.currentScene, 0);
    }
}
public void GoToStartScreen()
{
    
    SceneManager.LoadScene("1StartScreen");
}
// ... Other methods, if any, continue ...

    private IEnumerator FadeStateBar()
    {
         Debug.Log("Starting FadeStateBar Coroutine"); // Debug statement
        // Fade in or out the StateBar
        if (StateBar.activeSelf)
        {
            yield return FadeCanvasGroup(stateBarCanvasGroup, 0f, 1f, 0.2f);
            yield return new WaitForSeconds(0.6f);
            yield return FadeCanvasGroup(stateBarCanvasGroup, 1f, 0f, 0.2f);
            StateBar.SetActive(false);
        }
        isProcessing = false;
          Debug.Log("FadeStateBar Coroutine ended"); // Debug statement
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float duration)
    {
        Debug.Log("Starting FadeCanvasGroup Coroutine"); // Debug statement
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, time / duration);
            yield return null;
        }
        canvasGroup.alpha = end;
        Debug.Log("FadeCanvasGroup Coroutine ended"); // Debug statement
    }
       public void QuitGame()
    {
        // Quit the application logic here
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    
    
}
