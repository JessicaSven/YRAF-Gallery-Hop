using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPlace", menuName = "Place System/PlaceSO")]
public class PlaceSO : ScriptableObject
{
    public string PlaceName;
    public Sprite Icon;
    public Color Color;
    [TextArea(3, 10)]
    public string Content;
    [TextArea(3, 10)]
    public string RavenSucessScript;
    [TextArea(3, 10)]
    public string RavenHintScript;
    public string Url;
    public Vector2 Position;
    public List<string> VoteTitles;
}

