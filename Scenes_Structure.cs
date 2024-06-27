using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
[ CreateAssetMenu (fileName = "newstoryscene", menuName = "Data/New Story Scene")]
 [System.Serializable]
public class Scene_Structure : ScriptableObject
{
    public List<sentence> sentences;
    
 
    public Sprite background;
    public Scene_Structure nextscene;
     public Scene_Structure previousscene;
     public AudioClip BackgroundMusic;
    [System.Serializable]

    public struct sentence
    {
        public string text;
        public Speaker speaker;  
        public AudioClip voice;
    }
     
  
}
