using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lobby
{
    public class StartLocalManagers : MonoBehaviour
    {
        [SerializeField] private string sceneName = "LocalManagers";

        private void Awake()
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
    }
}