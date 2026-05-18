using UnityEngine;

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
    }
}
