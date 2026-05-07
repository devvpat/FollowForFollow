using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "F4F/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea(2, 4)] public string description;
}