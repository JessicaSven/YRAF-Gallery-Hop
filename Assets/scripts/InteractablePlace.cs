
using UnityEngine;
using UnityEngine.UI;

public class InteractablePlace : MonoBehaviour
{
    public PlaceSO place ;
    public Image image;
    internal void initialize(PlaceSO item)
    {
        place = item;
        image.sprite = item.icon;
    }
    public void Ontap()
    {
        UIController.instance.ShowPlaceDetails(place);
    }

}
