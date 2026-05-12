using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BuddySlotUI : MonoBehaviour
{
    public Image portraitImage;
    public TMP_Text ignText;
    public CharacterProfile assignedProfile;
    public CharacterProfilePanel profilePanel;

    [Header("Selection Feedback")]
    public GameObject selectionHighlight;

    static BuddySlotUI currentSelection;
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();
        button.onClick.AddListener(HandleClick);

        SetHighlight(false);
    }

    void Start()
    {
        if (assignedProfile == null)
            return;

        if (ignText != null)
            ignText.text = assignedProfile.ign;

        if (portraitImage != null && assignedProfile.portraitSprite != null)
            portraitImage.sprite = assignedProfile.portraitSprite;
    }

    void HandleClick()
    {
        if (assignedProfile == null)
            return;

        if (profilePanel == null)
            profilePanel = Object.FindFirstObjectByType<CharacterProfilePanel>();

        if (profilePanel != null)
            profilePanel.Show(assignedProfile);

        SetSelected();
    }

    void SetSelected()
    {
        if (currentSelection != null && currentSelection != this)
            currentSelection.SetHighlight(false);

        currentSelection = this;
        SetHighlight(true);
    }

    void SetHighlight(bool active)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(active);
    }

    void OnDisable()
    {
        if (currentSelection == this)
            currentSelection = null;
    }
}
