using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Reusable battle "juice" effects for a combatant's sprite: a hop (on acting), a shake + red flash
// (on taking damage). All restart-safe so rapid triggers never leave a sprite stuck off-position or
// tinted. Hop/Shake animate anchoredPosition (so the target must NOT be layout-group controlled);
// Flash tints the Image color. Coroutines run on a caller-provided host MonoBehaviour.
public static class BattleSpriteFx
{
    private const float HopHeight = 25f;
    private const float HopDuration = 0.25f;
    private const float ShakeMagnitude = 12f;
    private const float ShakeDuration = 0.25f;
    private const float FlashDuration = 0.3f;
    private static readonly Color FlashColor = new Color(1f, 0.25f, 0.25f, 1f);

    // One position effect (hop OR shake) at a time per RectTransform.
    private static readonly Dictionary<RectTransform, Coroutine> _posRunning = new();
    private static readonly Dictionary<RectTransform, Vector2> _posBase = new();
    // One flash at a time per Image.
    private static readonly Dictionary<Image, Coroutine> _flashRunning = new();
    private static readonly Dictionary<Image, Color> _flashBase = new();

    public static void Hop(MonoBehaviour host, RectTransform rt)
        => StartPos(host, rt, HopRoutine);

    public static void Shake(MonoBehaviour host, RectTransform rt)
        => StartPos(host, rt, ShakeRoutine);

    // Horizontal step toward foes (dirX = +1 right / -1 left) and back.
    public static void Lunge(MonoBehaviour host, RectTransform rt, float dirX)
    {
        if (host == null || rt == null || !host.isActiveAndEnabled) return;
        Vector2 basePos;
        if (_posRunning.TryGetValue(rt, out var existing) && existing != null)
        {
            host.StopCoroutine(existing);
            basePos = _posBase[rt];
            rt.anchoredPosition = basePos;
        }
        else
        {
            basePos = rt.anchoredPosition;
            _posBase[rt] = basePos;
        }
        _posRunning[rt] = host.StartCoroutine(LungeRoutine(rt, basePos, dirX));
    }

    // Per-frame subtle scale "breathing". Call from an Update loop; pass a phase to stagger.
    public static void Breathe(Transform t, float phase = 0f, float speed = 1.3f, float amplitude = 0.03f)
    {
        if (t == null) return;
        float s = 1f + Mathf.Sin(Time.time * speed + phase) * amplitude;
        t.localScale = new Vector3(s, s, 1f);
    }

    public static void Flash(MonoBehaviour host, Image img) => Flash(host, img, FlashColor);

    public static void Flash(MonoBehaviour host, Image img, Color flashColor)
    {
        if (host == null || img == null || !host.isActiveAndEnabled) return;
        Color baseColor;
        if (_flashRunning.TryGetValue(img, out var existing) && existing != null)
        {
            host.StopCoroutine(existing);
            baseColor = _flashBase[img];
            img.color = baseColor;
        }
        else
        {
            baseColor = img.color;
            _flashBase[img] = baseColor;
        }
        _flashRunning[img] = host.StartCoroutine(FlashRoutine(img, baseColor, flashColor));
    }

    private delegate IEnumerator PosEffect(RectTransform rt, Vector2 basePos);

    private static void StartPos(MonoBehaviour host, RectTransform rt, PosEffect effect)
    {
        if (host == null || rt == null || !host.isActiveAndEnabled) return;
        Vector2 basePos;
        if (_posRunning.TryGetValue(rt, out var existing) && existing != null)
        {
            host.StopCoroutine(existing);
            basePos = _posBase[rt];
            rt.anchoredPosition = basePos;
        }
        else
        {
            basePos = rt.anchoredPosition;
            _posBase[rt] = basePos;
        }
        _posRunning[rt] = host.StartCoroutine(effect(rt, basePos));
    }

    private static IEnumerator HopRoutine(RectTransform rt, Vector2 basePos)
    {
        float t = 0f;
        while (t < HopDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / HopDuration);
            rt.anchoredPosition = basePos + new Vector2(0f, Mathf.Sin(p * Mathf.PI) * HopHeight);
            yield return null;
        }
        EndPos(rt, basePos);
    }

    private static IEnumerator LungeRoutine(RectTransform rt, Vector2 basePos, float dirX)
    {
        const float dur = 0.28f;
        const float dist = 55f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            rt.anchoredPosition = basePos + new Vector2(Mathf.Sin(p * Mathf.PI) * dist * dirX, 0f);
            yield return null;
        }
        EndPos(rt, basePos);
    }

    private static IEnumerator ShakeRoutine(RectTransform rt, Vector2 basePos)
    {
        float t = 0f;
        while (t < ShakeDuration)
        {
            t += Time.deltaTime;
            float decay = 1f - Mathf.Clamp01(t / ShakeDuration);
            // Deterministic jitter (no Random — keeps it cheap and resume-safe).
            float ox = Mathf.Sin(t * 90f) * ShakeMagnitude * decay;
            float oy = Mathf.Cos(t * 75f) * ShakeMagnitude * decay;
            rt.anchoredPosition = basePos + new Vector2(ox, oy);
            yield return null;
        }
        EndPos(rt, basePos);
    }

    private static void EndPos(RectTransform rt, Vector2 basePos)
    {
        rt.anchoredPosition = basePos;
        _posRunning.Remove(rt);
        _posBase.Remove(rt);
    }

    private static IEnumerator FlashRoutine(Image img, Color baseColor, Color flashColor)
    {
        float t = 0f;
        while (t < FlashDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / FlashDuration);
            // Start at the flash color, lerp back to the base color.
            img.color = Color.Lerp(flashColor, baseColor, p);
            yield return null;
        }
        img.color = baseColor;
        _flashRunning.Remove(img);
        _flashBase.Remove(img);
    }

    // ----- Scale effects (localScale-based, so they're safe on layout-group children) -----

    private const float PunchDuration = 0.3f;
    private static readonly Dictionary<Transform, Coroutine> _scaleRunning = new();
    private static readonly Dictionary<Transform, Coroutine> _pulseRunning = new();

    // One-shot "punch": start at fromScale and ease back to 1 with a slight overshoot.
    public static void ScalePunch(MonoBehaviour host, Transform t, float fromScale)
    {
        if (host == null || t == null || !host.isActiveAndEnabled) return;
        PulseStop(t); // pulse and punch share the scale; don't fight
        if (_scaleRunning.TryGetValue(t, out var existing) && existing != null)
            host.StopCoroutine(existing);
        _scaleRunning[t] = host.StartCoroutine(PunchRoutine(t, fromScale));
    }

    private static IEnumerator PunchRoutine(Transform t, float fromScale)
    {
        float time = 0f;
        while (time < PunchDuration)
        {
            time += Time.deltaTime;
            float p = Mathf.Clamp01(time / PunchDuration);
            // Ease-out with a small overshoot near the end.
            float s = Mathf.Lerp(fromScale, 1f, p) + Mathf.Sin(p * Mathf.PI) * 0.08f;
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        t.localScale = Vector3.one;
        _scaleRunning.Remove(t);
    }

    // Looping gentle pulse (e.g. on the active actor). Call PulseStop to end + reset.
    public static void PulseStart(MonoBehaviour host, Transform t)
    {
        if (host == null || t == null || !host.isActiveAndEnabled) return;
        if (_pulseRunning.ContainsKey(t)) return; // already pulsing
        _pulseRunning[t] = host.StartCoroutine(PulseRoutine(t));
    }

    public static void PulseStop(Transform t)
    {
        if (t == null) return;
        if (_pulseRunning.TryGetValue(t, out var c))
        {
            // Stopping via the dictionary owner isn't possible here without the host; the coroutine
            // checks the dictionary each frame and exits when its entry is removed.
            _pulseRunning.Remove(t);
            t.localScale = Vector3.one;
        }
    }

    private static IEnumerator PulseRoutine(Transform t)
    {
        float time = 0f;
        while (_pulseRunning.ContainsKey(t) && t != null)
        {
            time += Time.deltaTime;
            float s = 1f + Mathf.Sin(time * 4f) * 0.04f; // subtle ±4% breathing
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        if (t != null) t.localScale = Vector3.one;
    }
}
