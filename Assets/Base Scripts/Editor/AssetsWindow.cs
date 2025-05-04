#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using Base_Scripts.AssetsWindow;    // namespace de votre AssetsConfig
using System.Linq;

[InitializeOnLoad]
public static class AssetsWindow
{
    private static AssetsConfig config;
    private static int selectedSceneIndex;
    private static int selectedPrefabIndex;
    private static string[] sceneNames = new string[0];
    private static string[] prefabNames = new string[0];
    private static bool isExpanded = true;    // foldout par défaut ouvert
    private static Rect windowRect = new Rect(10, 10, 240, 120);
    private const int windowId = 0xABCD;

    static AssetsWindow()
    {
        LoadConfig();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void LoadConfig()
    {
        var guids = AssetDatabase.FindAssets("t:AssetsConfig");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            config = AssetDatabase.LoadAssetAtPath<AssetsConfig>(path);
            RefreshLists();
        }
    }

    private static void RefreshLists()
    {
        if (config != null)
        {
            sceneNames = config.scenes?.Select(s => s != null ? s.name : "<Missing>").ToArray() ?? new string[0];
            prefabNames = config.prefabs?.Select(p => p != null ? p.name : "<Missing>").ToArray() ?? new string[0];
        }
        selectedSceneIndex = Mathf.Clamp(selectedSceneIndex, 0, sceneNames.Length - 1);
        selectedPrefabIndex = Mathf.Clamp(selectedPrefabIndex, 0, prefabNames.Length - 1);
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (config == null)
            LoadConfig();

        Handles.BeginGUI();
        windowRect = GUI.Window(windowId, windowRect, DrawWindow, "Assets Window");
        Handles.EndGUI();
    }

    private static void DrawWindow(int id)
    {
        // Scenes dropdown
        EditorGUILayout.LabelField("→ Choisir une scène", EditorStyles.boldLabel);
        if (sceneNames.Length > 0)
        {
            int newSceneIndex = EditorGUILayout.Popup(selectedSceneIndex, sceneNames);
            if (newSceneIndex != selectedSceneIndex)
            {
                selectedSceneIndex = newSceneIndex;
                OpenSceneAt(selectedSceneIndex);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Aucune scène configurée");
        }

        EditorGUILayout.Space();

        // Prefabs dropdown (ouverture immédiate)
        EditorGUILayout.LabelField("→ Choisir un prefab", EditorStyles.boldLabel);
        if (prefabNames.Length > 0)
        {
            int newPrefabIndex = EditorGUILayout.Popup(selectedPrefabIndex, prefabNames);
            if (newPrefabIndex != selectedPrefabIndex)
            {
                selectedPrefabIndex = newPrefabIndex;
                OpenPrefabAt(selectedPrefabIndex);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Aucun prefab configuré");
        }
        // Permet de déplacer la fenêtre en drag-and-drop sur la barre de titre
        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
    }

    private static void OpenSceneAt(int index)
    {
        var sceneAsset = config.scenes[index];
        if (sceneAsset == null)
        {
            Debug.LogError($"SceneAsset null à l'index {index}");
            return;
        }
        string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }

    private static void OpenPrefabAt(int index)
    {
        var go = config.prefabs[index];
        if (go == null)
        {
            Debug.LogError($"GameObject prefab null à l'index {index}");
            return;
        }
        string prefabPath = AssetDatabase.GetAssetPath(go);
        PrefabStageUtility.OpenPrefab(prefabPath);
    }
}
#endif
