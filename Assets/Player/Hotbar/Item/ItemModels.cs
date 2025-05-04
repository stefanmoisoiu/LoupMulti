using System;
using UnityEngine;

namespace Player.Hotbar.Item
{
    public class ItemModels : MonoBehaviour
    {
        [SerializeField] private ItemList itemList;
        [SerializeField] private GameObject[] itemModels = Array.Empty<GameObject>();
        [SerializeField] private Transform parent;
        

        #if UNITY_EDITOR
        public void UpdateItemModels()
        {
            for (int i = 0; i < itemModels.Length; i++) DestroyImmediate(itemModels[i]);
            
            itemModels = new GameObject[itemList.Items.Length];
            for (int i = 0; i < itemList.Items.Length; i++)
            {
                GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(itemList.Items[i].ItemData.model);
                if (instance == null)
                {
                    Debug.LogError($"Failed to instantiate prefab for item {itemList.Items[i].name}");
                    continue;
                }
                instance.transform.SetParent(parent, false);
                instance.transform.localPosition = Vector3.zero;
                instance.SetActive(false);
                itemModels[i] = instance;
            }
        }
        #endif
        
        public void ShowItemModel(Item showItem)
        {
            for (int i = 0; i < itemModels.Length; i++)
            {
                Item item = itemList.Items[i];
                itemModels[i].SetActive(item == showItem);
            }
        }
    }
    
    #if UNITY_EDITOR
    
    [UnityEditor.CustomEditor(typeof(ItemModels))]
    public class ItemModelsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Update Item Models"))
            {
                ((ItemModels)target).UpdateItemModels();
            }
        }
    }
    #endif
}