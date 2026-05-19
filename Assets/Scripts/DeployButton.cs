using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DeployButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Floor1";
    [SerializeField] private bool startDisabled = true;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Deploy);
        if (startDisabled)
            button.interactable = false;
    }

    public void EnableDeploy()
    {
        if (button == null)
            button = GetComponent<Button>();
        button.interactable = true;
    }

    public void Deploy()
    {
        Debug.Log($"DeployButton.Deploy() fired. targetSceneName='{targetSceneName}'");
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("DeployButton: targetSceneName is empty.");
            return;
        }
        SceneManager.LoadScene(targetSceneName);
    }
}
