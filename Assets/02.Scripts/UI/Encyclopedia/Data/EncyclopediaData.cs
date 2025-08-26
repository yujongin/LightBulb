using UnityEngine;
[System.Serializable]
public class EncyclopediaData
{
    public string itemKey;
    public string itemName;
    public ImageData[] itemImageData;
    public string[] itemDescription;
    public string[] lifeStyle;
}

[System.Serializable]
public class ImageData
{
    public string itemImage;
    public string authorship;
    public string sourceLink;
    public string CCBYLink;
}