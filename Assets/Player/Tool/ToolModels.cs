using System;
using System.Collections.Generic;
using Player.Model.Grabber;
using UnityEngine;

namespace Player.Tool
{
    public class ToolModels : MonoBehaviour
    {
        [SerializeField] private ToolList toolList;
        [SerializeField] private ToolModel[] toolModels;
        [SerializeField] private GrabberShowHideAnim showHideAnimator;
        [SerializeField] private Transform parent;

        #if UNITY_EDITOR
        public void GenerateToolModelsEditor()
        {
            foreach(Transform child in parent) 
            {
                if (child == null) continue;
                DestroyImmediate(child.gameObject);
            }
            
            List<ToolModel> m = new List<ToolModel>();
            
            for (int i = 0; i < toolList.Tools.Length; i++)
            {
                GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(toolList.Tools[i].ToolData.model);
                if (instance == null)
                {
                    Debug.LogError($"Failed to instantiate prefab for tool {toolList.Tools[i].name}");
                    continue;
                }
                instance.transform.SetParent(parent, false);
                instance.transform.localPosition = Vector3.zero; 
                instance.SetActive(false);
                
                m.Add(new(instance, GetRenderers(instance.transform).ToArray()));
            }
            toolModels = m.ToArray();
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        private List<Renderer> GetRenderers(Transform t)
        {
            List<Renderer> renderers = new List<Renderer>();
            if (t.TryGetComponent(out Renderer r)) renderers.Add(r);
            foreach (Transform child in t)
            {
                List<Renderer> childRenderers = GetRenderers(child);
                if (childRenderers != null) renderers.AddRange(childRenderers);
            }
            return renderers;
        }
        #endif

        private void Start()
        {
            foreach (var toolModel in toolModels)
                showHideAnimator.UpdateShowHide(toolModel.renderers, 1);
        }

        public void ChangeToolModel(ushort previousToolID, ushort newToolID)
        {
            if (previousToolID == newToolID) return;
            
            if (newToolID == ushort.MaxValue)
            {
                StartCoroutine(showHideAnimator.ShowHideTool(false, toolModels[previousToolID].renderers));
                StartCoroutine(showHideAnimator.ShowHideArm(false));
                return;
            }
            
            toolModels[newToolID].model.SetActive(true);
            
            if (previousToolID == ushort.MaxValue) StartCoroutine(showHideAnimator.ShowHideArm(true));
            else StartCoroutine(showHideAnimator.ShowHideTool(false, toolModels[previousToolID].renderers));
            StartCoroutine(showHideAnimator.ShowHideTool(true, toolModels[newToolID].renderers));
        }

        public ToolModel GetToolModel(ushort toolID) 
        {
            if (toolID == ushort.MaxValue) return null;
            if (toolID >= toolModels.Length)
            {
                Debug.LogError($"Tool ID {toolID} is out of range.");
                return null;
            }
            return toolModels[toolID];
        }
        public ToolModel GetToolModel(Tool tool) 
        {
            if (tool == null) return null;
            return GetToolModel(toolList.GetToolID(tool));
        }
    }
    
    #if UNITY_EDITOR
    
    [UnityEditor.CustomEditor(typeof(ToolModels))]
    public class ToolModelsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Update Tool Models"))
            {
                ((ToolModels)target).GenerateToolModelsEditor();
            }
        }
    }
    #endif
    
    [Serializable]
    public class ToolModel
    {
        public GameObject model;
        public Renderer[] renderers;
        
        public ToolModel(GameObject model, Renderer[] renderers)
        {
            this.model = model;
            this.renderers = renderers;
        }
    }
}

