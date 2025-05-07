using UnityEngine;

[CreateAssetMenu(fileName = "NewPlace", menuName = "Place System/PlaceSO")]
public class PlaceSO : ScriptableObject
{
    public string PlaceName;
    public Sprite Icon;
    public Color Color;
    public string Content;
    public string RavenSucessScript;
    public string RavenHintScript;
    public string Url;
    public Vector2 Position;
}

