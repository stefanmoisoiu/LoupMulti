using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameLoop : NetworkBehaviour
{
    public const int RoundCount = 2;
    public const int TimeToUpgrade = 10;
    public const int Countdown = 5;
    public const int GameLength = 30;
    
    
    public NetworkVariable<RoundState> roundState = new(RoundState.None);
    private Coroutine _gameLoopCoroutine;

    public Action<RoundState, float> OnRoundStateChanged;
    [Rpc(SendTo.Everyone)]
    private void OnRoundStateChangedClientRpc(RoundState state, float serverTime)
        => OnRoundStateChanged?.Invoke(state, serverTime);
    
    public void StartGameLoop()
    {
        if (_gameLoopCoroutine != null) StopCoroutine(_gameLoopCoroutine);
        _gameLoopCoroutine = StartCoroutine(MainLoop());
    }

    private IEnumerator MainLoop()
    {
        // Choose Upgrade -> Play Round -> Repeat
        int round = 1;

        while (round <= RoundCount)
        {
            NetcodeLogger.Instance.LogRpc("Round " + round, NetcodeLogger.ColorType.Green);

            GameManager.Instance.mapManager.SetPlayerSpawnPositions();
            yield return ChooseUpgrade();
            yield return PlayRound();
            round++;
        }

        yield return EndGame();
    }

    private IEnumerator ChooseUpgrade()
    {
        NetcodeLogger.Instance.LogRpc($"Choosing upgrade for {TimeToUpgrade} seconds...", NetcodeLogger.ColorType.Green);
        roundState.Value = RoundState.ChoosingUpgrade;
        
        OnRoundStateChangedClientRpc(RoundState.ChoosingUpgrade, NetworkManager.ServerTime.TimeAsFloat);

        GameManager.Instance.upgradesManager.ChooseUpgradesForPlayingPlayersServer();
        yield return new WaitForSeconds(TimeToUpgrade);
        GameManager.Instance.upgradesManager.UpgradeTimeFinishedServer();
        
        OnRoundStateChangedClientRpc(RoundState.ChoosingUpgrade, NetworkManager.ServerTime.TimeAsFloat);
    }
    private IEnumerator PlayRound()
    {
        NetcodeLogger.Instance.LogRpc($"Countdown of {Countdown} seconds", NetcodeLogger.ColorType.Green);
        roundState.Value = RoundState.Countdown;
        
        OnRoundStateChangedClientRpc(RoundState.Countdown, NetworkManager.ServerTime.TimeAsFloat);
        
        yield return new WaitForSeconds(Countdown);
        
        OnRoundStateChangedClientRpc(RoundState.Countdown, NetworkManager.ServerTime.TimeAsFloat);
        
        NetcodeLogger.Instance.LogRpc($"Playing round for {GameLength} seconds", NetcodeLogger.ColorType.Green);
        roundState.Value = RoundState.InRound;
        
        OnRoundStateChangedClientRpc(RoundState.InRound, NetworkManager.ServerTime.TimeAsFloat);
        yield return new WaitForSeconds(GameLength);
        
        OnRoundStateChangedClientRpc(RoundState.InRound, NetworkManager.ServerTime.TimeAsFloat);
    }
    private IEnumerator EndGame()
    {
        NetcodeLogger.Instance.LogRpc("Game ended", NetcodeLogger.ColorType.Green);
        roundState.Value = RoundState.None;
        OnRoundStateChangedClientRpc(RoundState.None, NetworkManager.ServerTime.TimeAsFloat);
        yield break;
    }
    
    public enum RoundState
    {
        None,
        Countdown,
        InRound,
        ChoosingUpgrade,
    }
}