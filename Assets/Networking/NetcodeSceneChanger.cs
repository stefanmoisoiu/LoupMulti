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
                if (netScenes[i].SceneName == sceneName) return netScenes[i];
            throw new Exception("Scene '" + sceneName + "' not found in NetcodeSceneChanger's netScenes array.");
        }
        
        public bool IsSceneLoaded(string sceneName, SceneType type)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            switch (type)
            {
                case SceneType.Active:
                    if (_activeNetScene == null) return false;
                    return _activeNetScene.SceneName == sceneName;
                case SceneType.Manager:
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
            
            if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) && !IsSpawned)
            {
                GetComponent<NetworkObject>().Spawn(true);
            }
            
            NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;

            NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading += VerifySceneBeforeLoading;
            // On a supprimé la ligne "VerifySceneBeforeLoading"
        }

        private bool VerifySceneBeforeLoading(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode)
        {
            try
            {
                SceneInfo eventInfo = GetSceneInfo(sceneName);
                return true;
            }
            catch (Exception)
            {
                return false;
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
            
            SceneInfo eventInfo;
            try {
                eventInfo = GetSceneInfo(sceneEvent.SceneName);
            }
            catch (Exception) {
                // Cette scène n'est pas gérée par ce manager (c'est peut-être une scène locale)
                // On l'ignore.
                return;
            }
            
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load:
                    Debug.Log("Loading network scene: " + sceneEvent.SceneName);
                    
                    if (!eventInfo.Transition) break;
                    sceneEvent.AsyncOperation.allowSceneActivation = false;
                    
                    IEnumerator TransitionInCoroutine()
                    {
                        Debug.Log($"Start transition IN {Time.time}");
                        yield return TransitionManager.Instance.TransitionCoroutine(true);
                        sceneEvent.AsyncOperation.allowSceneActivation = true;
                        Debug.Log($"End transition IN {Time.time}");
                    }
                    StartCoroutine(TransitionInCoroutine());
                    
                    break;
                
                case SceneEventType.LoadComplete:
                    Debug.Log("Loaded network scene: " + sceneEvent.SceneName);
                    
                    if (eventInfo.Type == SceneType.Active)
                    {
                        SceneManager.SetActiveScene(sceneEvent.Scene);
                        LocalSceneChanger.Instance.UnloadActiveLocalScene();
                    }

                    if (eventInfo.Transition)
                    {
                        IEnumerator TransitionOutCoroutine()
                        {
                            yield return new WaitForSeconds(0.5f);
                            Debug.Log($"Start transition OUT {Time.time}");
                            yield return TransitionManager.Instance.TransitionCoroutine(false);
                            yield return null;
                            Debug.Log($"End transition OUT {Time.time}");
                        }
                        StartCoroutine(TransitionOutCoroutine());
                    }
                    break;

                case SceneEventType.LoadEventCompleted:
                    // Déclenché uniquement sur le SERVEUR
                    Debug.Log("Load event completed for scene: " + sceneEvent.SceneName);
                    break;
                
                // On peut ignorer les autres événements pour l'instant
                case SceneEventType.Synchronize: break;
                case SceneEventType.SynchronizeComplete: break;
            }
        }

        public IEnumerator NetcodeLoadScene(string sceneName, SceneType type)
        {
            if (!NetworkManager.Singleton.IsServer) 
            {
                Debug.LogError("NetcodeLoadScene ne peut être appelé que par le serveur !");
                yield break;
            }
            
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
            // --- CHARGEMENT ---
            NetworkManager.Singleton.SceneManager.LoadScene(newSceneName, LoadSceneMode.Additive);
            
            bool loadCompleted = false;
            void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
            {
                if (sceneName == newSceneName) loadCompleted = true;
            }
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            yield return new WaitUntil(() => loadCompleted);
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;

            Debug.Log($"[Server] Chargement de '{newSceneName}' terminé sur tous les clients.");

            // --- DÉCHARGEMENT ---
            if (previousActiveNetScene != null)
            {
                Debug.Log($"[Server] Déchargement de la scène précédente '{previousActiveNetScene.SceneName}'.");
                yield return null; // Attendre 1 frame
                
                NetworkManager.Singleton.SceneManager.UnloadScene(
                    SceneManager.GetSceneByName(previousActiveNetScene.SceneName));

                bool unloadCompleted = false;
                void OnUnloadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
                {
                    if (sceneName == previousActiveNetScene.SceneName) unloadCompleted = true;
                }
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += OnUnloadEventCompleted;
                yield return new WaitUntil(() => unloadCompleted);
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= OnUnloadEventCompleted;
                
                Debug.Log($"[Server] Déchargement de '{previousActiveNetScene.SceneName}' terminé sur tous les clients.");
            }
        }
    }
}