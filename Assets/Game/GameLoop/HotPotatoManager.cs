using System;
using Unity.Netcode;
using UnityEngine;

public class HotPotatoManager : NetworkBehaviour
{
    public NetworkVariable<ulong> target = new(ulong.MaxValue);

    [SerializeField] private ushort healthLossPerSec = 5;
    [SerializeField] private ushort healthLossTickDelay = 10;
    
    public bool HotPotatoActiveServer { get; private set; }

    private void Start()
    {
        if (!IsServer) return;

        PlayerDeath.OnPlayerDiedServer += _ => SetTarget(GetRandomAliveClientId());
    }

    public void ActivateRandomHotPotato() => ActivateHotPotato(GetRandomAliveClientId());
    public void ActivateHotPotato(ulong id)
    {
        SetTarget(id);
        if (HotPotatoActiveServer) return;
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
    
    private void SetTarget(ulong id)
    {
        target.Value = id;
        target.SetDirty(true);
        Debug.LogError("Set target to " + id);
    }
    private void ResetTarget() => SetTarget(ulong.MaxValue);
    private void ApplyHotPotato()
    {
        if (GameTickManager.CurrentTick % healthLossTickDelay != 0) return;
        if (target.Value == ulong.MaxValue) return;
        GameData gameData = GameManager.Instance.GameData;
        if (!gameData.PlayerDataManager.TryGetValue(target.Value, out PlayerData data)) return;
        ushort damage = (ushort)(healthLossPerSec * healthLossTickDelay /
                                 GameTickManager.TICKRATE);
        gameData.PlayerDataManager[target.Value] = new(data) {InGameData = data.InGameData.RemoveHealth(damage)};
    }
    
    private ulong GetRandomAliveClientId()
    {
        ulong[] keys = GameManager.Instance.GameData.PlayerDataManager.GetKeys();
        ushort randomIndex;
        if (keys.Length == 0) return ulong.MaxValue;
        do
        {
            randomIndex = (ushort)UnityEngine.Random.Range(0, keys.Length);
        }
        while (!GameManager.Instance.GameData.PlayerDataManager[randomIndex].InGameData.IsAlive());
        return keys[randomIndex];
    }
}