using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnOrderIconUI : MonoBehaviour
{
    public Image portraitImage;
    public Image borderImage;
    public TMP_Text initialLabel;

    [Header("Colors")]
    public Color allyBorderColor = new Color(0.45f, 0.35f, 0.70f, 1f);
    public Color enemyBorderColor = new Color(0.70f, 0.25f, 0.30f, 1f);
    public Color currentActorGlow = new Color(1f, 0.85f, 0.3f, 1f);

    private BattleCharacter _char;

    public void Bind(BattleCharacter character)
    {
        _char = character;
        if (initialLabel != null)
            initialLabel.text = character.Name.Length > 0 ? character.Name[0].ToString() : "?";
        if (borderImage != null)
            borderImage.color = character is Ally ? allyBorderColor : enemyBorderColor;
        if (portraitImage != null)
            portraitImage.color = character is Ally ? new Color(0.55f, 0.40f, 0.75f, 1f) : new Color(0.75f, 0.30f, 0.35f, 1f);
    }

    public void SetCurrentActor(bool isCurrent)
    {
        if (borderImage != null)
        {
            if (isCurrent)
                borderImage.color = currentActorGlow;
            else
                borderImage.color = _char is Ally ? allyBorderColor : enemyBorderColor;
        }
    }
}
