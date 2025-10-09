using System;
using Networking;
using Networking.Connection;
using UnityEngine;

namespace Lobby
{
    public class MultiplayerTest : MonoBehaviour
    {
        [SerializeField] private string multLobbySceneName = "MultiLobby";

        [SerializeField] private Type type;

        private bool joining;

        enum Type
        {
            Create,
            Join
        }

        private void Start()
        {
            if (type == Type.Create)
            {
                Create();
            }
            else if (type == Type.Join)
            {
                TryJoin();
            }
        }

        private void Update()
        {
            if (type == Type.Join) TryJoin();
        }
        private async void Create()
        {
            try
            {
                await NetcodeManager.Instance.CreateGame();
                GUIUtility.systemCopyBuffer = NetcodeManager.Instance.CurrentServerJoinCode;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void TryJoin()
        {
            if (joining) return; // clipboard copy
            string code = GUIUtility.systemCopyBuffer;
            Debug.Log(code);
            if (code is not { Length: 6 }) return;
        
            Join(code);
        }

        private async void Join(string joinCode)
        {
            joining = true;
            try
            {
                await NetcodeManager.Instance.JoinGame(joinCode);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            joining = false;
        }
    }
}
