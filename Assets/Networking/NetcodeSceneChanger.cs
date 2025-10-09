

using System;
using System.Collections;
using System.Collections.Generic;
using Rendering.Transitions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class NetcodeSceneChanger : NetworkBehaviour
    {
        private SceneInfo _activeNetScene;
        private List<SceneInfo> _loadedNetManagerScenes = new();
        
        [SerializeField] private SceneInfo[] netScenes;
        public static NetcodeSceneChanger Instance { get; private set; }
        
        public SceneInfo GetSceneInfo(string sceneName)
        {
            for (int i = 0; i < netScenes.Length; i++) 
                if (netScenes[i].Scene.name == sceneName) return netScenes[i];
            throw new Exception("Scene not found in scenes array: " + sceneName);
        }
        
        public bool IsSceneLoaded(string sceneName, SceneType type)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            switch (type)
            {
                case SceneType.Active:
                    if (_activeNetScene == null) return false;
                    return _activeNetScene.Scene.name == sceneName;
                case SceneType.Manager:
                    Debug.Log("Checking if manager scene is loaded: " + sceneName);
                    SceneInfo eventInfo = GetSceneInfo(sceneName);
                    foreach (var scene in _loadedNetManagerScenes)
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
            if ((IsHost || IsServer) && !IsSpawned) GetComponent<NetworkObject>().Spawn(true);
            
            NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
            NetworkManager.SceneManager.VerifySceneBeforeLoading += VerifyNetScene;
        }
        private bool VerifyNetScene(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode)
        {
            Debug.Log("Verifying scene for network load: " + sceneName + " (Index: " + sceneIndex + ", Mode: " + loadSceneMode + ")");
            try
            {
                SceneInfo info = LocalSceneChanger.Instance.GetSceneInfo(sceneName);
                Debug.Log($"sceneName was found in local scenes : {LocalSceneChanger.Instance.IsSceneLoaded(info.Scene.name, info.Type)}, IsNetwork: {info.IsNetwork}");
                return info.IsNetwork;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        
        private void OnDisable()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
        }
        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.ClientId != NetworkManager.Singleton.LocalClientId) return;
            if (sceneEvent.SceneName == null)
            {
                Debug.LogError("SceneEvent with null SceneName");
                return;
            }
            SceneInfo eventInfo = GetSceneInfo(sceneEvent.SceneName);
            
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load:
                    Debug.Log("Loading scene: " + sceneEvent.SceneName);
                    
                    if (!eventInfo.Transition) break;
                    sceneEvent.AsyncOperation.allowSceneActivation = false;
                    
                    IEnumerator TransitionInCoroutine()
                    {
                        yield return TransitionManager.Instance.TransitionCoroutine(true);
                        sceneEvent.AsyncOperation.allowSceneActivation = true;
                    }
                    StartCoroutine(TransitionInCoroutine());
                    
                    break;
                case SceneEventType.Synchronize:
                    Debug.Log("Synchronizing scene: " + sceneEvent.SceneName);
                    break;
                case SceneEventType.SynchronizeComplete:
                    Debug.Log("Synchronizing COMPLETE scene: " + sceneEvent.SceneName);
                    break;

                case SceneEventType.LoadComplete:
                    Debug.Log("Loaded scene: " + sceneEvent.SceneName);
                    
                    if (eventInfo.Type == SceneType.Active)
                    {
                        SceneManager.SetActiveScene(sceneEvent.Scene);
                        LocalSceneChanger.Instance.UnloadActiveLocalScene();
                    }
                    break;

                case SceneEventType.LoadEventCompleted:
                    Debug.Log("Load event completed for scene: " + sceneEvent.SceneName);
                    // Déclenché uniquement sur le SERVEUR lorsque TOUS les clients ont fini de charger.
                    
                    if (eventInfo.Transition)
                    {
                        TransitionManager.Instance.TransitionCoroutine(false);
                    }
                    break;
            }
        }

        public IEnumerator NetcodeLoadScene(string sceneName, SceneType type)
        {
            if (IsSceneLoaded(sceneName, type)) yield break;
            
            SceneInfo info = GetSceneInfo(sceneName);
            SceneInfo previousActiveNetScene = null;

            if (type == SceneType.Active)
            {
                previousActiveNetScene = _activeNetScene;
                _activeNetScene = info;
            }
            else if (type == SceneType.Manager)
            {
                _loadedNetManagerScenes.Add(info);
            }

            yield return StartCoroutine(ServerLoadAndUnload(sceneName, previousActiveNetScene));
        }

        private IEnumerator ServerLoadAndUnload(string newSceneName, SceneInfo previousActiveNetScene)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(newSceneName, LoadSceneMode.Additive);

            // Attendre que la scène soit complètement chargée sur tous les clients
            bool loadCompleted = false;
            void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
            {
                if (sceneName == newSceneName) loadCompleted = true;
            }
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            yield return new WaitUntil(() => loadCompleted);
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;

            Debug.Log($"[Server] Chargement de '{newSceneName}' terminé sur tous les clients.");

            if (previousActiveNetScene != null)
            {
                Debug.Log($"[Server] Déchargement de la scène précédente '{previousActiveNetScene.Scene.name}'.");
                //Attendre 1 frame pour éviter le SceneEventInProgress après LoadEventCompleted.
                yield return null; 
                
                NetworkManager.Singleton.SceneManager.UnloadScene(
                    SceneManager.GetSceneByName(previousActiveNetScene.Scene.name));

                bool unloadCompleted = false;
                void OnUnloadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
                {
                    if (sceneName == previousActiveNetScene.Scene.name) unloadCompleted = true;
                }
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += OnUnloadEventCompleted;
                yield return new WaitUntil(() => unloadCompleted);
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= OnUnloadEventCompleted;
                
                Debug.Log($"[Server] Déchargement de '{previousActiveNetScene.Scene.name}' terminé sur tous les clients.");
            }
        }
    }
}