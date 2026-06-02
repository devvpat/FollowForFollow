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

    public static void Flash(MonoBehaviour host, Image img)
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
        _flashRunning[img] = host.StartCoroutine(FlashRoutine(img, baseColor));
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

    private static IEnumerator FlashRoutine(Image img, Color baseColor)
    {
        float t = 0f;
        while (t < FlashDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / FlashDuration);
            // Start fully red, lerp back to the base color.
            img.color = Color.Lerp(FlashColor, baseColor, p);
            yield return null;
        }
        img.color = baseColor;
        _flashRunning.Remove(img);
        _flashBase.Remove(img);
    }
}
