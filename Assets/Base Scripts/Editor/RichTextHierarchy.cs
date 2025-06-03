using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Base_Scripts.Editor
{
    [InitializeOnLoad]
public static class RichTextHierarchy
{
    static RichTextHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    // regex pour les tags en début de nom : hex (6 chiffres), nommés (red, green…) et gras (b)
    private static readonly Regex hexColorRegex   = new Regex(@"^#([0-9A-Fa-f]{6})");
    private static readonly Regex namedColorRegex = new Regex(@"^#(red|green|blue|yellow|cyan|magenta|white|black|grey|gray)", RegexOptions.IgnoreCase);

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;

        // 1) on cache tout
        bool even = ((int)(selectionRect.y / EditorGUIUtility.singleLineHeight)) % 2 == 0;
        Color bg = even ? new(0.6f, 0.6f, 0.6f) : new(0.7f, 0.7f, 0.7f);
        EditorGUI.DrawRect(selectionRect, bg);

        // 2) on parse tous les tags #… en tête de nom
        string name = go.name;
        Color textColor = GUI.color;
        bool bold = false;

        bool again;
        do
        {
            again = false;
            // hex : #RRGGBB
            var mh = hexColorRegex.Match(name);
            if (mh.Success)
            {
                if (ColorUtility.TryParseHtmlString("#" + mh.Groups[1].Value, out var c))
                    textColor = c;
                name = name.Substring(mh.Length);
                again = true;
                continue;
            }

            // nommée : #red, #green…
            var mn = namedColorRegex.Match(name);
            if (mn.Success)
            {
                textColor = GetNamedColor(mn.Groups[1].Value.ToLower());
                name = name.Substring(mn.Length);
                again = true;
                continue;
            }

            // gras : #b
            if (name.StartsWith("#b"))
            {
                bold = true;
                name = name.Substring(2);
                again = true;
            }
        }
        while (again);

        // 3) préparation du style
        var style = new GUIStyle(EditorStyles.label)
        {
            fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            clipping  = TextClipping.Clip,
        };

        // 4) dessine l’icône
        var content = EditorGUIUtility.ObjectContent(go, typeof(GameObject));
        var iconRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.height, selectionRect.height);
        GUI.DrawTexture(iconRect, content.image, ScaleMode.ScaleToFit);

        // 5) dessine le nom stylisé
        var textRect = new Rect(
            iconRect.xMax + 2,
            selectionRect.y + (selectionRect.height - EditorGUIUtility.singleLineHeight) * 0.5f,
            selectionRect.width - iconRect.width - 2,
            EditorGUIUtility.singleLineHeight
        );
        style.normal.textColor = Color.white;
        GUI.color = Color.Lerp(textColor, Color.white, 0f);
        EditorGUI.LabelField(textRect, name, style);

        // 6) reset
        GUI.color = Color.white;
    }

    private static Color GetNamedColor(string n)
    {
        switch (n)
        {
            case "red":     return Color.red;
            case "green":   return Color.green;
            case "blue":    return Color.blue;
            case "yellow":  return Color.yellow;
            case "cyan":    return Color.cyan;
            case "magenta": return Color.magenta;
            case "white":   return Color.white;
            case "black":   return Color.black;
            case "grey":
            case "gray":    return Color.gray;
            default:        return Color.white;
        }
    }
}

}