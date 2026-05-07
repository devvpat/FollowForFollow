using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DeployButton : MonoBehaviour
{
    [Header("Target Scene")]
    public string dungeonSceneName = "SampleScene";

    [Header("UI")]
    public TMP_Text statusLabel;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Deploy);
    }

    void Update()
    {
        int count = PartyManager.Instance != null ? PartyManager.Instance.selected.Count : 0;
        button.interactable = count > 0;
        if (statusLabel != null)
            statusLabel.text = count == 0 ? "Pick a party" : $"Deploy ({count})";
    }

    void Deploy()
    {
        if (PartyManager.Instance == null || PartyManager.Instance.selected.Count == 0)
            return;
        SceneManager.LoadScene(dungeonSceneName);
    }
}