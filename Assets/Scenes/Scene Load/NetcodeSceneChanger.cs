using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetcodeSceneChanger : MonoBehaviour
{
    public static NetcodeSceneChanger Instance { get; private set; }
    [SerializeField] private TransitionManager transitionManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientStarted += InitalizeTransitions;
    }

    private void InitalizeTransitions()
    {
        NetworkSceneManager sm = NetworkManager.Singleton.SceneManager;
        sm.OnSynchronize += async clientId =>
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            StartCoroutine(transitionManager.Transition(true, () =>
            {
                tcs.SetResult(true);;
            }));
            await tcs.Task;
        };
        sm.OnSynchronizeComplete += async clientId =>
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            
            StartCoroutine(transitionManager.Transition(false, null));
        };
        sm.OnLoad += (clientId, sceneName, loadMode, asyncOp) =>
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            
            asyncOp.allowSceneActivation = false;

            StartCoroutine(transitionManager.Transition(true, () =>
            {
                asyncOp.allowSceneActivation = true;
            }));
        };
        sm.OnLoadComplete += (clientId, sceneName, mode) =>
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            
            StartCoroutine(transitionManager.Transition(false, null));
        };
    }
    public async void LocalChangeScene(string sceneName)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        StartCoroutine(transitionManager.Transition(true, () => tcs.SetResult(true)));
        await tcs.Task;
        await SceneManager.LoadSceneAsync(sceneName);
        StartCoroutine(transitionManager.Transition(false, null));
    }

    public void NetworkChangeScene(string sceneName)
    {
        if (sceneName == SceneManager.GetActiveScene().name) return;
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}