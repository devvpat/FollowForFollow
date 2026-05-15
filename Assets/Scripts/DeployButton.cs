using UnityEngine;
using UnityEngine.SceneManagement;

public class DeployButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "SampleScene";

    public void Deploy()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("DeployButton: targetSceneName is empty.");
            return;
        }
        SceneManager.LoadScene(targetSceneName);
    }
}
