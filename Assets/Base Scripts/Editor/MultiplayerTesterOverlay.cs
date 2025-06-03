#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base_Scripts.Editor
{
    // 1) On déclare un Overlay dockable dans la SceneView
    [Overlay(typeof(SceneView), "MultTest", defaultDockZone = DockZone.RightColumn)]
    public class MultiplayerTesterOverlay : Overlay
    {
        // 2) Chemin constant vers la scène à lancer
        private const string CreateScenePath = "Assets/Scenes/CreateTest.unity";
        private const string JoinScenePath = "Assets/Scenes/JoinTest.unity";

        // 3) On construit l’arborescence UI via UI Toolkit
        public override VisualElement CreatePanelContent()
        {
            // Conteneur principal
            var root = new VisualElement();
            
            root.style.flexDirection = FlexDirection.Row;

            // Bouton Create
            var CreateButton = new Button(() =>
            {
                // 4) Sauvegarde et chargement de la scène, puis Play Mode
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(CreateScenePath, OpenSceneMode.Single);
                    EditorApplication.EnterPlaymode();
                }
            })
            {
                text = "+"
            };
            CreateButton.style.width = 24;
            CreateButton.style.height = 24;
            root.Add(CreateButton);
            
            // Bouton Join
            var JoinButton = new Button(() =>
            {
                // 4) Sauvegarde et chargement de la scène, puis Play Mode
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(JoinScenePath, OpenSceneMode.Single);
                    EditorApplication.EnterPlaymode();
                }
            })
            {
                text = ">"
            };
            JoinButton.style.width = 24;
            JoinButton.style.height = 24;
            root.Add(JoinButton);

            return root;
        }
    }
}
#endif