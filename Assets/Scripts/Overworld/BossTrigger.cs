using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private GameObject battleUI;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PauseController.SetPause(true);
            battleUI.SetActive(true);
            BattleManager.Instance.StartNewFight();
        }
    }
}
