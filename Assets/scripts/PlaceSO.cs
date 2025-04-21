using UnityEngine;

[CreateAssetMenu(fileName = "NewPlace", menuName = "Place System/PlaceSO")]
public class PlaceSO : ScriptableObject
{
    public string placeName;
    public string description;
    public Sprite icon;
    public Vector2 position;
}
