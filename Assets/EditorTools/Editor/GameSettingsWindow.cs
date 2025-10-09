using Game.Common;
using UnityEditor;
using UnityEngine;

public class GameSettingsWindow : EditorWindow
{
    private GameSettings gameSettings;
    
        [MenuItem("Window/Game Settings")]
        public static void ShowWindow()
        {
            GetWindow<GameSettingsWindow>("Game Settings");
        }
    
        private void OnEnable()
        {
            gameSettings = Resources.Load("GameSettings" ) as GameSettings;
        }

        private void OnGUI()
        {
            if (gameSettings == null)
            {
                EditorGUILayout.HelpBox("Aucun GameSettings trouvé à l'emplacement spécifié.", MessageType.Warning);
                return;
            }
            SerializedObject serializedObject = new SerializedObject(gameSettings);
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true); // Skip the script field
            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }
            serializedObject.ApplyModifiedProperties();
        }
}
