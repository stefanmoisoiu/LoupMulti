#if UNITY_EDITOR
#if MK_URP
using Plugins._MK.MKToon.Editor;
using UnityEditor;
using UnityEngine;

namespace Plugins._MK.MKToon.Scripts.URP.Editor
{
    [CustomEditor(typeof(MKToonPerObjectOutlines))]
    public class MKToonPerObjectOutlinesEditor : UnityEditor.Editor
    {
        private GUIContent _layerMaskUI = new GUIContent
        (
            "Layer Mask",
            "Defines the layermask for objects, which should render the outline, if a MK Toon outlined shader is applied."
        );

        private SerializedProperty _layerMask;

        private void UpdateInstallWizard()
        {
            InstallWizard installWizard = InstallWizard.instance;
            if(installWizard != null)
                InstallWizard.instance.Repaint();
        }

        private void OnEnable()
        {
            UpdateInstallWizard();
        }

        private void OnDisable()
        {
            UpdateInstallWizard();
        }

        private void FindProperties()
        {
            _layerMask = serializedObject.FindProperty("_layerMask");
        }

        public override void OnInspectorGUI()
        {
            FindProperties();
            
            EditorGUILayout.PropertyField(_layerMask, _layerMaskUI);
            //DrawDefaultInspector();

            UpdateInstallWizard();
        }
    }
}
#endif
#endif