using UnityEngine;
using UnityEngine.SceneManagement;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private GameObject battleUI;
    [SerializeField] private EnemyParty enemyParty;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PauseController.SetPause(true);
        battleUI.SetActive(true);
        BattleManager.Instance.StartNewFight(enemyParty.Enemies);
        BattleManager.Instance.OnBattleEnd += BattleEnd;
    }

    private void BattleEnd(bool playerWon)
    {
        BattleManager.Instance.OnBattleEnd -= BattleEnd;

        AllyParty.Instance.ResetAllAlliesHealthAndMana();
        PauseController.SetPause(false);

        if (playerWon)
        {
            gameObject.SetActive(false);   
        }
    }
}
