#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Common
{
    [CustomEditor(typeof(PerkData))]
    [CanEditMultipleObjects]
    public class PerkDataEditor : UnityEditor.Editor
    {
        private SerializedProperty iconProp;
        private SerializedProperty nameProp;
        private SerializedProperty descProp;
        private SerializedProperty rarityProp;

        private void OnEnable()
        {
            iconProp   = serializedObject.FindProperty("perkIcon");
            nameProp   = serializedObject.FindProperty("perkName");
            descProp   = serializedObject.FindProperty("perkDescription");
            rarityProp = serializedObject.FindProperty("perkRarity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Couleur selon la rareté
            Color rarityColor = GetColorForRarity(rarityProp.enumValueIndex);

            // Bandeau supérieur
            Rect bannerRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 50);
            EditorGUI.DrawRect(bannerRect, rarityColor);

            // Affiche l'icône à gauche si présente
            Sprite sprite = iconProp.objectReferenceValue as Sprite;
            if (sprite != null)
            {
                // zone 36×36 px à 7 px du bord
                Rect iconRect = new Rect(bannerRect.x + 7, bannerRect.y + 7, 36, 36);
                // dessine la texture (gère transparence)
                EditorGUI.DrawTextureTransparent(iconRect, sprite.texture);
            }

            // Titre centré (décalé si icône présente)
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 14,
                normal    = { textColor = Color.white }
            };
            string title = nameProp.stringValue != "" 
                ? nameProp.stringValue 
                : ObjectNames.NicifyVariableName(target.GetType().Name);

            // Si icône ok, on élève le label pour éviter chevauchement
            Rect titleRect = bannerRect;
            titleRect.xMin += (sprite != null ? 50 : 0);
            EditorGUI.LabelField(titleRect, title, titleStyle);

            GUILayout.Space(8);

            // Corps de l'inspecteur
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.PropertyField(iconProp,  new GUIContent("Icon"));
                EditorGUILayout.PropertyField(nameProp,  new GUIContent("Name"));
                EditorGUILayout.PropertyField(descProp,  new GUIContent("Description"));
                EditorGUILayout.PropertyField(rarityProp, new GUIContent("Rarity"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private Color GetColorForRarity(int index)
        {
            switch ((PerkData.PerkRarity)index)
            {
                case PerkData.PerkRarity.Common:    return new Color32(160, 160, 160, 255);
                case PerkData.PerkRarity.Uncommon:  return new Color32( 80, 200,  80, 255);
                case PerkData.PerkRarity.Rare:      return new Color32( 80, 150, 230, 255);
                case PerkData.PerkRarity.Epic:      return new Color32(180,  80, 220, 255);
                case PerkData.PerkRarity.Legendary: return new Color32(255, 200,  50, 255);
                default:                            return Color.white;
            }
        }
    }
}
#endif
