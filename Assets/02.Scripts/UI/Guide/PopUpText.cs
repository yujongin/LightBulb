using UnityEngine;

[CreateAssetMenu(fileName = "PopUpText", menuName = "Scriptable Objects/PopUpText")]
public class PopUpText : ScriptableObject
{
    public PopUpTextType type;
    public string conditionName;
    public string text;
}

public enum PopUpTextType
{
    Success,
    Fail,
    Guide
}
