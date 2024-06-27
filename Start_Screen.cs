using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;  // This is required for the Serializable attribute

public class StartingScreen : MonoBehaviour
{

    public Button quitButton, newgameButton, configurationsButton, loadgameButton, D1Button, D2Button, HelpButton, Closehelp; 
    public GameObject ConfigurationBar, StateBar, DecisionBar, HelpBar;
    public CanvasGroup fadePanel, stateBarCanvasGroup; // Added stateBarCanvasGroup
    public AudioSource backgroundMusic;
    public AudioClip selectedMusic;
    public TextMeshProUGUI D1Text, D2Text, DecisionTitle, StateText; // Ensure StateText is linked in the Inspector
    private bool isMuted = false;
    private bool isProcessing = false; // Declare the isProcessing variable
    public TextMeshProUGUI StateTitle;  // Add this line
    public Button CloseConfig;

public TMP_Dropdown screenModeDropdown;
public Slider SizeSlider, textSpeedSlider, musicVolumeSlider, voiceVolumeSlider;
public AudioSource musicAudioSource, voiceAudioSource;
private bool configIsActive = false;
private int tempWidth, tempHeight;
 [Serializable]
public class SaveData
{
    public float textSpeed;
    public float musicVolume;
    public float voiceVolume;
    public int screenMode;
    public float windowSize; 
    public int windowWidth;
    public int windowHeight;

}
    void Start()
    {
        // Set screen resolution
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        ConfigurationBar.SetActive(false);
        StateBar.SetActive(false);
        DecisionBar.SetActive(false);
        HelpBar.SetActive(false);
        // Add listeners to buttons
        AddButtonListeners();
        // Start the fade effect
        StartCoroutine(FadeScreenIn());
        PlayBackgroundMusic(); // Start playing background music
        InitializeUI();
        AssignButtonListeners();
        string path = Application.persistentDataPath + "/savegame.json";

    if (System.IO.File.Exists(path))
    {
        string json = System.IO.File.ReadAllText(path);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        textSpeedSlider.value = saveData.textSpeed;
        musicVolumeSlider.value = saveData.musicVolume;
        voiceVolumeSlider.value = saveData.voiceVolume;
        screenModeDropdown.value = saveData.screenMode; // Apply screen mode setting
        
       
        SetTextSpeed(saveData.textSpeed);
        SetMusicVolume(saveData.musicVolume);
        SetVoiceVolume(saveData.voiceVolume);
        OnScreenModeChange(saveData.screenMode); // Call OnScreenModeChange to apply the screen mode
        SizeSlider.value = saveData.windowSize;
        // tempWidth = PlayerPrefs.GetInt("WindowWidth", Screen.currentResolution.width / 2);
        // tempHeight = PlayerPrefs.GetInt("WindowHeight", Screen.currentResolution.height / 2);
        // Screen.SetResolution(tempWidth, tempHeight, false);
        ApplySizeChange();
    }
     else
    {
        textSpeedSlider.value = 0.5f;
        musicVolumeSlider.value = 0.5f;
        voiceVolumeSlider.value = 0.5f;
        //SizeSlider.value = 0.5f;

        SetTextSpeed(0.5f);
        SetMusicVolume(0.5f);
        SetVoiceVolume(0.5f);
        OnScreenModeChange(0); // Default to fullscreen
      
    }
    }
   

    private void AssignButtonListeners()
{
    // Assign listeners to configuration-related buttons and sliders
    CloseConfig.onClick.AddListener(CloseConfiguration);
    screenModeDropdown.onValueChanged.AddListener(OnScreenModeChange);
    textSpeedSlider.onValueChanged.AddListener(delegate { SetTextSpeed(textSpeedSlider.value); });
    musicVolumeSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicVolumeSlider.value); });
    voiceVolumeSlider.onValueChanged.AddListener(delegate { SetVoiceVolume(voiceVolumeSlider.value); });
    SizeSlider.onValueChanged.AddListener(OnSizeChanged);
    AddPointerUpListener(SizeSlider, ApplySizeChange);
}
public void SetTextSpeed(float speed)
{
    float mappedSpeed = Mathf.Lerp(0.1f, 0.01f, speed);
    // if (dialogueManager != null)
    // {
    //     dialogueManager.SetTextSpeed(mappedSpeed);
    // }
    PlayerPrefs.SetFloat("TextSpeed", speed); // Save the value
    PlayerPrefs.Save(); // Ensure it's saved
}

public void SetMusicVolume(float volume)
{
    musicAudioSource.volume = volume;
    PlayerPrefs.SetFloat("MusicVolume", volume);
    PlayerPrefs.Save();
     Debug.Log("Music Volume Saved: " + volume);
}

public void SetVoiceVolume(float volume)
{
    voiceAudioSource.volume = volume;
    PlayerPrefs.SetFloat("VoiceVolume", volume);
    PlayerPrefs.Save();
     Debug.Log("Voice Volume Saved: " + volume);
}

private void AddPointerUpListener(Slider slider, UnityEngine.Events.UnityAction call)
{
    EventTrigger trigger = slider.GetComponent<EventTrigger>() ?? slider.gameObject.AddComponent<EventTrigger>();
    trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.PointerUp);
    EventTrigger.Entry entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerUp;
    entry.callback.AddListener((data) => { call(); });
    trigger.triggers.Add(entry);
}
 
public void OnScreenModeChange(int modeIndex)
{
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
            PlayerPrefs.SetInt("WindowWidth", tempWidth);
            PlayerPrefs.SetInt("WindowHeight", tempHeight);
            PlayerPrefs.SetFloat("WindowSize", SizeSlider.value);
            PlayerPrefs.Save();
        }
    }
private void UpdateSlider()
{
    float currentValue = (float)Screen.width / Screen.currentResolution.width;
    SizeSlider.value = currentValue;
}

private void InitializeUI()
{
    // Initialize configuration-related UI elements
    textSpeedSlider.value = 0.5f;
    musicVolumeSlider.value = 0.5f;
    voiceVolumeSlider.value = 0.5f;
    ConfigurationBar.SetActive(false);
    SizeSlider.gameObject.SetActive(false);
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
  void Update()
{
     if (Input.GetKeyDown(KeyCode.M))  // Check if the 'M' key was pressed
    {
        ToggleMute();  // Call the ToggleMute method
    }
    // Check if the Q key was pressed
    if (Input.GetKeyDown(KeyCode.Q)||Input.GetKeyDown(KeyCode.Escape))
    {
        DecisionCloseGame(); // Attempt to quit the game
    }
    
    // Check if the N key was pressed
    if (Input.GetKeyDown(KeyCode.N))
    {
        NewGame(); // Start a new game
    }
    
    // Check if the L key was pressed
    if (Input.GetKeyDown(KeyCode.L))
    {
        LoadGame(); // Load a game
    }
    
    // Check if the C key was pressed
    if (Input.GetKeyDown(KeyCode.C))
    {
        Configuration(); // Open configurations
    }
    
    // Check if the H key was pressed
    if (Input.GetKeyDown(KeyCode.H))
    {
        Help(); // Open help
    }
}

  public void ToggleMute()
{
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
private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
{
    float counter = 0f;
    while (counter < duration)
    {
        counter += Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, counter / duration);
        yield return null;
    }
    canvasGroup.alpha = endAlpha;
}

   private void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && selectedMusic != null)
        {
            backgroundMusic.clip = selectedMusic; // Assign the selected music clip to the AudioSource
            backgroundMusic.Play(); // Play the assigned music clip
        }
        else
        {
            // if (backgroundMusic == null)
            //     Debug.LogError("AudioSource component not assigned.");
            // if (selectedMusic == null)
            //     Debug.LogError("No music clip selected in the Inspector.");
        }
    }
   
    private void AddButtonListeners()
    {
            newgameButton.onClick.AddListener(NewGame); 
            loadgameButton.onClick.AddListener(LoadGame); 
            configurationsButton.onClick.AddListener(Configuration); 
            quitButton.onClick.AddListener(DecisionCloseGame); 
            HelpButton.onClick.AddListener(Help);
        
    }

      public void NewGame()
    { 
        Debug.Log("NewGame method called.");
        // Start the fade-out effect, then load the new scene
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(FadeScreenOut("2Name"));
    }

public void LoadGame()
{
    Debug.Log("LoadGame method called.");
    SceneManager.sceneLoaded += OnSceneLoaded;
    SceneManager.LoadScene("3Conversation");
    Debug.Log("LoadGame method completed.");
}


// Define the callback method
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "3Conversation")
    {
        // Remove the callback to prevent it from being called again unnecessarily
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Use a coroutine to delay the execution of LoadGame until the end of the frame
        StartCoroutine(DelayedLoadGame());
    }
    if (scene.name == "2Name")
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Destroy(gameObject);
    }
}

private IEnumerator DelayedLoadGame()
{
   
    yield return new WaitForEndOfFrame();

   
        UI_Functionality uiFunctionality = FindObjectOfType<UI_Functionality>();
        if (uiFunctionality != null)
        {
            uiFunctionality.LoadGame(); // This should set up the dialogue manager's state
        }
        else
        {
            Debug.LogError("UI_Functionality script not found in the scene.");
        }
    Destroy(gameObject);
} 


void Awake() {
    DontDestroyOnLoad(gameObject);
}



    // Open settings
    public void Settings()
    {
        Debug.Log("Settings method called.");
    }
    public void Help()
    {
        bool helpisactive = HelpBar.activeSelf;
        if(!helpisactive){
        HelpBar.SetActive(true);
         Closehelp.onClick.AddListener(() => HelpBar.SetActive(false));
        }
        else
        HelpBar.SetActive(false);
    }

public void DecisionCloseGame(){
     bool decisionisactive = DecisionBar.activeSelf;
    if(!decisionisactive){
    DecisionBar.SetActive(true);
    D1Text.text = "Yes";
    D2Text.text = "No";
    DecisionTitle.text = "Are you sure you want to quit?";
    D1Button.onClick.AddListener(() => QuitGame());
    D2Button.onClick.AddListener(() => DecisionBar.SetActive(false));
    }
    else
     DecisionBar.SetActive(false);
}
    // Quit the game
    public void QuitGame()
    {
    #if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
    #else
    Application.Quit();
    #endif
    }

    // Remove listeners when GameObject is destroyed
    void OnDestroy()
    {
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }
    }


     // Coroutine to fade the screen in
    IEnumerator FadeScreenIn()
    {
        float duration = 2.0f;
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
     IEnumerator FadeScreenOut(string sceneName)
    {
        fadePanel.gameObject.SetActive(true); // Make sure the panel is active
        float duration = 2.0f;
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

 
}
