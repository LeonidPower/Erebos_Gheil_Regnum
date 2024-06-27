using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMusic : MonoBehaviour
{
    public AudioSource audioSource;  // Drag your AudioSource component here in the inspector
    public AudioClip clip;           // Drag your AudioClip here in the inspector
    public float delayInSeconds = 0; // Set this value in the inspector to delay the start of the music

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); // Adds an AudioSource component if not already attached
        }
        audioSource.clip = clip;      // Set the audio clip to the one selected in the inspector
        audioSource.playOnAwake = false; // Disable play on awake to manage play timing manually

        StartCoroutine(PlayMusicAfterDelay()); // Start the coroutine to delay music playback
    }

    IEnumerator PlayMusicAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds); // Wait for the specified delay
        audioSource.Play();           // Starts playing the audio after the delay
    }
}
