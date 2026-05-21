using UnityEngine;

public class BattleTestButton : MonoBehaviour
{
    public EnemyParty EnemyPartyRef;
    public GameObject InfoButton;
    public void OnClickTest()
    {
        gameObject.SetActive(false);
        BattleManager.Instance.StartNewFight(EnemyPartyRef.Enemies);
    }
    public void ToggleInfoButton()
    {
        InfoButton.SetActive(!InfoButton.activeSelf);
    }
}
