using UnityEngine;
using System.Collections.Generic;

public class TurnOrderUI : MonoBehaviour
{
    public Transform container;
    public GameObject turnIconPrefab;

    private List<TurnOrderIconUI> _icons = new();

    public void Refresh()
    {
        Clear();
        if (BattleManager.Instance == null) return;

        var turnOrder = BattleManager.Instance.GetPredictedTurnOrder(8);
        foreach (var character in turnOrder)
        {
            var go = Instantiate(turnIconPrefab, container);
            var icon = go.GetComponent<TurnOrderIconUI>();
            icon.Bind(character);
            _icons.Add(icon);
        }

        if (_icons.Count > 0)
            _icons[0].SetCurrentActor(true);
    }

    public void Clear()
    {
        foreach (var icon in _icons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        _icons.Clear();
    }
}
