using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DeployButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Floor1";

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Deploy);
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
