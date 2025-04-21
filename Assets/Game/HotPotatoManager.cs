using System;
using Unity.Netcode;
using UnityEngine;

public class HotPotatoManager : NetworkBehaviour
{
    public NetworkVariable<ulong> target = new(ulong.MaxValue);

    [SerializeField] private ushort healthLossPerSec = 5;
    [SerializeField] private ushort healthLossTickDelay = 10;
    
    public bool HotPotatoActiveServer { get; private set; }
    // SERVER
    public void ActivateHotPotato(ulong id)
    {
        if (HotPotatoActiveServer) return;
        SetTargetServer(id);
        GameTickManager.OnTickServer += ApplyHotPotato;
        HotPotatoActiveServer = true;
    }
    public void DeactivateHotPotato()
    {
        if (!HotPotatoActiveServer) return;
        ResetTarget();
        GameTickManager.OnTickServer -= ApplyHotPotato;
        HotPotatoActiveServer = false;
    }
    
    private void SetTargetServer(PlayerData playerData) => SetTargetServer(playerData.ClientId);
    private void SetTargetServer(ulong id)
    {
        target.Value = id;
        target.SetDirty(true);
    }
    private void ResetTarget() => SetTargetServer(ulong.MaxValue);
    private void ApplyHotPotato()
    {
        if (GameTickManager.CurrentTick % healthLossTickDelay != 0) return;
        if (target.Value == ulong.MaxValue) return;
        GameData gameData = GameManager.Instance.GameData;
        PlayerData playerData = gameData.PlayerGameData.GetDataOrDefault(target.Value);
        if (playerData.ClientId == ulong.MaxValue) return;
        playerData = new(playerData)
        {
            InGameData =
                playerData.InGameData.RemoveHealth((ushort)(healthLossPerSec * healthLossTickDelay /
                                                            GameTickManager.TICKRATE))
        };
        gameData.SetPlayerGameData(gameData.PlayerGameData.AddOrUpdateData(playerData));
    }
}