using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private GameObject battleUI;
    [SerializeField] private EnemyParty enemyParty;
    [SerializeField] private GameObject boss;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered || !collision.CompareTag("Player")) return;
        hasTriggered = true;

        if (TryGetComponent(out NPC npc))
        {
            Debug.Log("Starting boss dialogue");
            npc.OnDialogueEnded += StartBossFight;
            if (npc.TryStartDialogue())
            {
                return;
            }

            npc.OnDialogueEnded -= StartBossFight;
        }

        StartBossFight();
    }

    private void StartBossFight()
    {
        if (TryGetComponent(out NPC npc))
        {
            npc.OnDialogueEnded -= StartBossFight;
        }

        PauseController.SetPause(true);
        battleUI.SetActive(true);
        BattleManager.Instance.StartNewFight(enemyParty.Enemies);
        MusicManager.Instance.PlayMusic(MusicManager.Instance.battleTheme);

        BattleManager.Instance.OnBattleEnd += BattleEnd;
    }

    private void BattleEnd(bool playerWon)
    {
        BattleManager.Instance.OnBattleEnd -= BattleEnd;

        AllyParty.Instance.ResetAllAlliesHealthAndMana();
        PauseController.SetPause(false);
        MusicManager.Instance.PlayMusic(MusicManager.Instance.overworldTheme);

        if (playerWon)
        {
            if (boss != null)
            {
                boss.SetActive(false);
            }
            gameObject.SetActive(false);
        } else
        {
            hasTriggered = false;
        }
    }
}
