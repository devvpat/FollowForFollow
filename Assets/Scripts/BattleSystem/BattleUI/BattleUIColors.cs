using UnityEngine;

public static class BattleUIColors
{
    public static readonly Color PanelBackground = new Color(0.27f, 0.20f, 0.40f, 0.90f);
    public static readonly Color PanelBorder = new Color(0.55f, 0.40f, 0.75f, 1f);

    public static readonly Color HPBarFill = new Color(0.91f, 0.47f, 0.44f, 1f);   // coral/salmon
    public static readonly Color MPBarFill = new Color(0.49f, 0.66f, 0.85f, 1f);   // sky blue
    public static readonly Color BarBackground = new Color(0.27f, 0.26f, 0.29f, 1f); // dark gray track

    public static readonly Color ActiveAllyHighlight = new Color(0.70f, 0.55f, 1f, 0.6f);
    public static readonly Color TargetHighlight = new Color(1f, 0.85f, 0.1f, 1f);

    public static readonly Color NameTagBackground = new Color(0.80f, 0.74f, 0.86f, 1f); // light lavender
    public static readonly Color NameTagText = new Color(0.55f, 0.47f, 0.63f, 1f);       // muted mauve
}
