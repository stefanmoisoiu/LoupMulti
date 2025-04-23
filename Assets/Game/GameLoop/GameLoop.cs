using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameLoop : NetworkBehaviour
{
    [SerializeField] private HotPotatoManager hotPotatoManager;
    public HotPotatoManager HotPotatoManager => hotPotatoManager;
    [SerializeField] private GameTickManager gameTickManager;
    public GameTickManager GameTickManager => gameTickManager;
    [SerializeField] private PlayerDeath playerDeath;
    public PlayerDeath PlayerDeath => playerDeath;
    
    
    public const int RoundCount = 2;
    public const int TimeToUpgrade = 10;
    public const int Countdown = 5;
    
    
    public NetworkVariable<RoundState> roundState = new(RoundState.None);
    private Coroutine _gameLoopCoroutine;

    public static event Action<RoundState, float> OnRoundStateChanged;
    [Rpc(SendTo.Everyone)]
    private void OnRoundStateChangedClientRpc(RoundState state, float serverTime)
        => OnRoundStateChanged?.Invoke(state, serverTime);

    public static event Action OnGameEndedServer;


    private void Awake()
    {
        hotPotatoManager = GetComponent<HotPotatoManager>();
        gameTickManager = GetComponent<GameTickManager>();
    }

    public void StartGameLoop(GameManager manager)
    {
        if (_gameLoopCoroutine != null) StopCoroutine(_gameLoopCoroutine);
        _gameLoopCoroutine = StartCoroutine(MainLoop(manager));
    }

    private IEnumerator MainLoop(GameManager manager)
    {
        // Choose Upgrade -> Play Round -> Repeat
        int round = 1;
        
        gameTickManager.StartTickLoop();

        while (round <= RoundCount)
        {
            NetcodeLogger.Instance.LogRpc("Round " + round, NetcodeLogger.LogType.GameLoop);

            manager.MapManager.SetPlayerSpawnPositions();
            yield return ChooseUpgrade(manager);
            yield return PlayCountdown(manager);
            yield return PlayRound(manager);
            round++;
        }
        
        gameTickManager.StopTickLoop();

        yield return EndGame(manager);
    }

    private IEnumerator ChooseUpgrade(GameManager manager)
    {
        NetcodeLogger.Instance.LogRpc($"Choosing upgrade for {TimeToUpgrade} seconds...", NetcodeLogger.LogType.GameLoop);
        roundState.Value = RoundState.ChoosingUpgrade;
        
        OnRoundStateChangedClientRpc(RoundState.ChoosingUpgrade, NetworkManager.ServerTime.TimeAsFloat);

        GameManager.Instance.UpgradesManager.ChooseUpgradesForPlayersServer();
        yield return new WaitForSeconds(TimeToUpgrade);
        GameManager.Instance.UpgradesManager.ApplyUpgrades();
    }
    private IEnumerator PlayCountdown(GameManager manager)
    {
        NetcodeLogger.Instance.LogRpc($"Countdown of {Countdown} seconds", NetcodeLogger.LogType.GameLoop);
        roundState.Value = RoundState.Countdown;
        
        OnRoundStateChangedClientRpc(RoundState.Countdown, NetworkManager.ServerTime.TimeAsFloat);
        
        yield return new WaitForSeconds(Countdown);
    }
    private IEnumerator PlayRound(GameManager manager)
    {
        NetcodeLogger.Instance.LogRpc($"Playing round until 1 player is left", NetcodeLogger.LogType.GameLoop);
        roundState.Value = RoundState.InRound;
        OnRoundStateChangedClientRpc(RoundState.InRound, NetworkManager.ServerTime.TimeAsFloat);
        
        hotPotatoManager.ActivateRandomHotPotato();
        
        bool gameOver = false;
        void OnePlayerLeft(ulong clientId) {if (playerDeath.GetAlivePlayingPlayers().Length <= 1) gameOver = true;}
        
        PlayerDeath.OnPlayerDiedServer += OnePlayerLeft;
        yield return new WaitUntil(() => gameOver);
        PlayerDeath.OnPlayerDiedServer -= OnePlayerLeft;
        hotPotatoManager.DeactivateHotPotato();
    }

    private IEnumerator EndGame(GameManager manager)
    {
        NetcodeLogger.Instance.LogRpc("Game ended", NetcodeLogger.LogType.GameLoop);
        roundState.Value = RoundState.None;
        OnRoundStateChangedClientRpc(RoundState.None, NetworkManager.ServerTime.TimeAsFloat);
        OnGameEndedServer?.Invoke();
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