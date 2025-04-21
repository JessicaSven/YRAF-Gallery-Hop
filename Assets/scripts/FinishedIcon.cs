using System;
using UnityEngine;
using UnityEngine.UI;

public class FinishedIcon : MonoBehaviour
{
    public Image image;
    internal void initalize(PlaceSO temp)
    {
        image.sprite = temp.icon;
    }
}
