
using UnityEngine;
[CreateAssetMenu (fileName = "NewSpeaker", menuName ="Data/NewSpeaker")]
[System.Serializable]
public class Speaker : ScriptableObject
{
    public string speakerName;
    public Color textColor;
     public Sprite characterSprite;
}
