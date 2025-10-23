using System;
using System.Collections;
using System.Collections.Generic;
using Rendering.Transitions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class LocalSceneChanger : MonoBehaviour
    {
        private SceneInfo _activeLocalScene;
        private List<SceneInfo> _loadedLocalManagerScenes = new();
        
        public static LocalSceneChanger Instance { get; private set; }
        
        [SerializeField] private SceneInfo[] localScenes;

        public SceneInfo GetSceneInfo(string sceneName)
        {
            for (int i = 0; i < localScenes.Length; i++) 
                if (localScenes[i].SceneName == sceneName) return localScenes[i];
            throw new Exception("Scene not found in scenes array: " + sceneName);
        }
        
        public bool IsSceneLoaded(string sceneName, SceneType type)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            switch (type)
            {
                case SceneType.Active:
                    return SceneManager.GetActiveScene().name == sceneName;
                case SceneType.Manager:
                    SceneInfo eventInfo = GetSceneInfo(sceneName);
                    foreach (var scene in _loadedLocalManagerScenes)
                        if (scene == eventInfo)
                            return true;
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void Start()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _activeLocalScene = GetSceneInfo(SceneManager.GetActiveScene().name);
        }

        public void UnloadActiveLocalScene()
        {
            if (_activeLocalScene != null)
            {
                SceneManager.UnloadSceneAsync(_activeLocalScene.SceneName);
                _activeLocalScene = null;
            }
        }
         public IEnumerator LocalLoadScene(string sceneName, SceneType type)
        {
            Debug.Log("Starting local scene load: " + sceneName + " of type " + type);
            if (IsSceneLoaded(sceneName, type)) yield break;
            
            SceneInfo sceneInfo = GetSceneInfo(sceneName);

            switch (type)
            {
                case SceneType.Active:
                    if (_activeLocalScene == sceneInfo) yield break;
                    
                    SceneInfo previousLocalScene = _activeLocalScene;
                    _activeLocalScene = sceneInfo;
                    
                    if (sceneInfo.Transition)
                    {
                        yield return TransitionManager.Instance.TransitionCoroutine(true);
                    }
                    
                    yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
                    
                    Debug.Log("Local scene loaded: " + _activeLocalScene.SceneName);
                    
                    if (previousLocalScene != null)
                    {
                        Debug.Log("Unloading previous local scene: " + previousLocalScene.SceneName);
                        yield return SceneManager.UnloadSceneAsync(previousLocalScene.SceneName);
                    }
                    
                    if (sceneInfo.Transition)
                    {
                        yield return TransitionManager.Instance.TransitionCoroutine(false);
                    }
                    break;
                case SceneType.Manager:
                    yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    _loadedLocalManagerScenes.Add(sceneInfo);
                    Debug.Log("Local manager scene loaded: " + sceneInfo.SceneName);
                    break;
            }
        }
    }
}