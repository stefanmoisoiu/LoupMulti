#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
// namespace de votre AssetsConfig
using System.Linq;

namespace Base_Scripts.Editor
{
    [Overlay(typeof(SceneView), "Assets Window", defaultDockZone = DockZone.LeftColumn)]
    public class AssetsWindowOverlay : Overlay
    {
        private AssetsConfig _config;
        private string[] _sceneNames = new string[0];
        private string[] _prefabNames = new string[0];

        public override VisualElement CreatePanelContent()
        {
            LoadConfig();

            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    paddingLeft    = 4,
                    paddingTop     = 4,
                    paddingRight   = 4,
                    paddingBottom  = 4,
                    overflow       = Overflow.Hidden,
                    justifyContent = Justify.SpaceBetween
                }
            };

            // --- Scenes Section ---
            root.Add(new Label("→ Choisir une scène")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 }
            });

            var scenesContainer = new ScrollView();
            foreach (var (name, idx) in _sceneNames.Select((n, i) => (n, i)))
            {
                var btn = new Button(() => OpenSceneAt(idx))
                {
                    text = name,
                    tooltip = $"Charger la scène « {name} »",
                };
                btn.style.marginBottom = 2;
                scenesContainer.Add(btn);
            }
            root.Add(scenesContainer);

            root.Add(new VisualElement { style = { height = 8 } });

            // --- Prefabs Section ---
            root.Add(new Label("→ Choisir un prefab")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 }
            });

            var prefabsContainer = new ScrollView();
            foreach (var (name, idx) in _prefabNames.Select((n, i) => (n, i)))
            {
                var btn = new Button(() => OpenPrefabAt(idx))
                {
                    text = name,
                    tooltip = $"Ouvrir le prefab « {name} »",
                };
                btn.style.marginBottom = 2;
                prefabsContainer.Add(btn);
            }
            root.Add(prefabsContainer);

            return root;
        }

        private void LoadConfig()
        {
            if (_config != null) return;

            var guids = AssetDatabase.FindAssets("t:AssetsConfig");
            if (guids.Length == 0) return;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _config = AssetDatabase.LoadAssetAtPath<AssetsConfig>(path);
            RefreshLists();
        }

        private void RefreshLists()
        {
            if (_config == null) return;

            _sceneNames = _config.scenes?
                .Select(s => s != null ? s.name : "<Missing>")
                .ToArray() 
                ?? new string[0];

            _prefabNames = _config.prefabs?
                .Select(p => p != null ? p.name : "<Missing>")
                .ToArray() 
                ?? new string[0];
        }

        private void OpenSceneAt(int index)
        {
            if (_config == null || index < 0 || index >= _config.scenes.Count) return;

            var asset = _config.scenes[index];
            if (asset == null) { Debug.LogError($"SceneAsset null à l'index {index}"); return; }

            var path = AssetDatabase.GetAssetPath(asset);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        private void OpenPrefabAt(int index)
        {
            if (_config == null || index < 0 || index >= _config.prefabs.Count) return;

            var asset = _config.prefabs[index];
            if (asset == null) { Debug.LogError($"Prefab null à l'index {index}"); return; }

            var path = AssetDatabase.GetAssetPath(asset);
            PrefabStageUtility.OpenPrefab(path);
        }
    }
}
#endif
