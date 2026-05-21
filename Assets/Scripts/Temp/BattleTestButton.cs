using UnityEngine;

public class BattleTestButton : MonoBehaviour
{
    public EnemyParty EnemyPartyRef;
    public void OnClickTest()
    {
        gameObject.SetActive(false);
        BattleManager.Instance.StartNewFight(EnemyPartyRef.Enemies);
    }
}
