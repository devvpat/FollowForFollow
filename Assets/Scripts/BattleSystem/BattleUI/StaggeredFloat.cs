using System.Collections.Generic;
using UnityEngine;

// Gently floats a set of named child RectTransforms up and down on a sine wave, each offset in phase
// by its index so they bob in a staggered pattern. Attach to a parent (e.g. the battle-UI root); it
// resolves the targets by name, so no per-child wiring is needed. Animates anchoredPosition.y, so the
// targets must not be driven by a layout group.
public class StaggeredFloat : MonoBehaviour
{
    public string[] targetNames = { "EnemyDetailPanel", "errorback", "errorback (1)" };
    public float amplitude = 15f;
    public float speed = 1.5f;
    public float phaseStep = 2.094f; // ~2*PI/3 — even stagger for 3 elements

    private readonly List<RectTransform> _targets = new();
    private readonly List<Vector2> _basePositions = new();

    private void OnEnable()
    {
        _targets.Clear();
        _basePositions.Clear();
        if (targetNames == null) return;
        foreach (var name in targetNames)
        {
            var child = transform.Find(name);
            if (child is RectTransform rt)
            {
                _targets.Add(rt);
                _basePositions.Add(rt.anchoredPosition);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            var rt = _targets[i];
            if (rt == null) continue;
            float y = _basePositions[i].y + Mathf.Sin(Time.time * speed + i * phaseStep) * amplitude;
            rt.anchoredPosition = new Vector2(_basePositions[i].x, y);
        }
    }
}
